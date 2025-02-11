using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using MarBasBrokerSQLCommon.Access;
using MarBasBrokerSQLCommon.Grain;
using MarBasBrokerSQLCommon.GrainDef;
using MarBasBrokerSQLCommon.GrainTier;
using MarBasCommon;
using MarBasCommon.Reflection;
using MarBasSchema;
using MarBasSchema.Access;
using MarBasSchema.Broker;
using MarBasSchema.Grain;
using MarBasSchema.Grain.Traits;
using MarBasSchema.GrainDef;
using MarBasSchema.GrainTier;
using MarBasSchema.Transport;
using Microsoft.Extensions.Logging;

namespace MarBasBrokerSQLCommon.BrokerImpl
{
    public abstract class GrainTransportBroker<TDialect> :
        CloningBroker<TDialect>, IGrainTransportBroker, IAsyncGrainTransportBroker
        where TDialect : ISQLDialect, new()
    {
        class ImportGrainState
        {
            public ImportGrainState(IGrainTransportable grain, bool success = false)
            {
                Grain = grain;
                Success = false;
            }

            public IGrainTransportable Grain;
            public bool Success;
        };

        #region Variables
        protected const string SourceOperationImport = "Import";
        #endregion

        #region Construction
        protected GrainTransportBroker(IBrokerProfile profile, ILogger logger)
            : base(profile, logger)
        {
        }

        protected GrainTransportBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger logger)
            : base(profile, context, accessService, logger)
        {
        }
        #endregion

        #region Public Interface

        public int UpdateGrainTimestamps(IEnumerable<IIdentifiable> grains, DateTime? timestamp = null)
        {
            return UpdateGrainTimestampsAsync(grains, timestamp).Result;
        }

        public async Task<int> UpdateGrainTimestampsAsync(IEnumerable<IIdentifiable> grains, DateTime? timestamp = null, CancellationToken cancellationToken = default)
        {
            if (!grains.Any())
            {
                return 0;
            }
            CheckProfile();
            return await WrapInTransaction(0, async (ta) =>
            {
                return await UpdateGrainTimestampsInTA(ta, grains, timestamp, cancellationToken: cancellationToken);
            }, cancellationToken);
        }

        public IEnumerable<IGrainTransportable> ExportGrains(IEnumerable<IIdentifiable> grains)
        {
            return ExportGrainsAsync(grains).Result;
        }

        public async Task<IEnumerable<IGrainTransportable>> ExportGrainsAsync(IEnumerable<IIdentifiable> grains, CancellationToken cancellationToken = default)
        {
            var result = new List<IGrainTransportable>();
            if (!grains.Any())
            {
                return result;
            }
            CheckProfile();
            if (!await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.ExportSchema | RoleEntitlement.ReadAcl, false, cancellationToken))
            {
                throw new UnauthorizedAccessException("Not entitled to export from schema");
            }
            result = await ExecuteOnConnection(result, async (cmd) =>
            {
                using (cmd)
                {
                    var vals = grains.Select((x, index) =>
                    {
                        var paramName = $"{GeneralEntityDefaults.ParamId}{index}";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(paramName, x.Id));
                        return paramName;
                    });
                    cmd.CommandText = $"{GrainExtendedConfig<TDialect>.SQLSelectByAcl}g.{GeneralEntityDefaults.FieldId} IN (@{string.Join(",@", vals)})";
                    _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, (await _accessService.GetContextPrimaryRoleAsync(cancellationToken)).Id);

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await rs.ReadAsync(cancellationToken))
                        {
                            var grain = new GrainTransportable(new GrainExtendedDataAdapter(rs, GrainExtendedDataAdapter.ExtensionColumn.Type | GrainExtendedDataAdapter.ExtensionColumn.Path));
                            result.Add(grain);
                        }
                    }
                }
                return result;

            }, cancellationToken);

            await Parallel.ForEachAsync(result, new ParallelOptions { MaxDegreeOfParallelism = 3, CancellationToken = cancellationToken }, async (grain, token) =>
            {
                if (null == grain.TypeDefId)
                {
                    grain.Tier = await GetGrainTierTypeDef(grain, token);
                    if (null == grain.Tier && _logger.IsEnabled(LogLevel.Warning))
                    {
                        _logger.LogWarning("Grain {id} appears to be TypeDef but TypeDef specific data is missing", grain.Id);
                    }
                }
                else
                {
                    grain.Tier = await GetGrainTierPropDef(grain, token);
                    if (null == grain.Tier && !token.IsCancellationRequested)
                    {
                        grain.Tier = await GetGrainTierFile(grain, token);
                    }
                }
                grain.Acl = await GetGrainAclTransportable(grain, token);

                if (!token.IsCancellationRequested)
                {
                    var traitMap = await GetGrainTraitsTransportable(grain, token);
                    if (traitMap.TryGetValue(string.Empty, out IEnumerable<ITraitTransportable>? traits))
                    {
                        grain.Traits = traits;
                    }
                    if (!token.IsCancellationRequested)
                    {
                        grain.Localized = await GetGrainLocalizedLayers(grain, traitMap, token);
                    }
                }
            });

            return result;
        }

        public IGrainImportResults ImportGrains(IEnumerable<IGrainTransportable> grains, IEnumerable<IIdentifiable>? grainsToDelete = null, DuplicatesHandlingStrategy duplicatesHandling = DuplicatesHandlingStrategy.Merge)
        {
            return ImportGrainsAsync(grains, grainsToDelete, duplicatesHandling).Result;
        }

        public async Task<IGrainImportResults> ImportGrainsAsync(IEnumerable<IGrainTransportable> grains, IEnumerable<IIdentifiable>? grainsToDelete = null, DuplicatesHandlingStrategy duplicatesHandling = DuplicatesHandlingStrategy.Merge, CancellationToken cancellationToken = default)
        {
            GrainImportResults result = new();
            if (!grains.Any() && true != grainsToDelete?.Any())
            {
                return result;
            }
            CheckProfile();
            if (!await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.ImportSchema | RoleEntitlement.WriteAcl | RoleEntitlement.DeleteAcl | RoleEntitlement.SkipPermissionCheck, false, cancellationToken))
            {
                throw new UnauthorizedAccessException("Not entitled to import into schema");
            }

            var deleteFailures = new HashSet<IIdentifiable>();
            if (!cancellationToken.IsCancellationRequested && true == grainsToDelete?.Any())
            {
                var deleteList = grainsToDelete.ToList();
                var retries = Math.Max(2, deleteList.Count / 10);

                async Task<bool> DeleteWorkerFunc(IIdentifiable grain, int retry = 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Deleting grain {id} (pass {retry})", grain.Id, retry);
                    }
                    var error = await ImportDeleteGrain(grain, cancellationToken);
                    if (null == error)
                    {
                        result.DeletedCount++;
                        deleteFailures.Remove(grain);
                    }
                    else
                    {
                        if (0 == retry)
                        {
                            deleteFailures.Add(grain);
                        }
                        else if (retry >= retries)
                        {
                            result.AddFeedback(error);
                        }
                    }
                    return true;
                }

                foreach (var grain in deleteList)
                {
                    if (!await DeleteWorkerFunc(grain))
                    {
                        break;
                    }
                }
                for (int i = 1, failureCount = deleteFailures.Count; i <= retries && 0 < failureCount && !cancellationToken.IsCancellationRequested; i++)
                {
                    foreach (var grain in deleteFailures.ToList())
                    {
                        if (!await DeleteWorkerFunc(grain, i))
                        {
                            break;
                        }
                    }
                    if (failureCount == deleteFailures.Count)
                    {
                        // no more successful retries, skip the rest
                        i = retries;
                    }
                }
            }

            if (!cancellationToken.IsCancellationRequested && grains.Any())
            {
                var schema = await GetGrainAsync(SchemaDefaults.SchemaContainerID, cancellationToken: cancellationToken);
                if (null == schema)
                {
                    result.AddFeedback(new BrokerOperationFeedback($"Schema root grain ({SchemaDefaults.SchemaContainerID}) not found", SourceOperationImport, 404, LogLevel.Critical, SchemaDefaults.SchemaContainerID));
                    return result;
                }

                var schemaPath = schema.Path ?? string.Empty;
                var schemaGrainsByPath = new SortedDictionary<(DateTime TStamp, string Path), ImportGrainState>();
                var otherGrainsByPath = new SortedDictionary<(DateTime TStamp, string Path), ImportGrainState>();
                Parallel.ForEach(grains, (grain) =>
                {
                    var coll = true == grain.Path?.StartsWith(schemaPath) ? schemaGrainsByPath : otherGrainsByPath;
                    lock (coll)
                    {
                        coll.Add((grain.CTime, grain.Path ?? string.Empty), new(grain));
                    }
                });

                int maxRetries = (400 > grains.Count() ? 2 : (int)(Math.Log(grains.Count()) / 2)) - 1;
                var prevPassFailures = 0;
                async Task<bool> ImportWorkerFunc(ImportGrainState item, int pass = 0)
                {
                    if (cancellationToken.IsCancellationRequested || (0 < pass && 0 == prevPassFailures))
                    {
                        return false;
                    }
                    if (item.Success)
                    {
                        return true;
                    }
                    if (true == grainsToDelete?.Any(x => x.Id == item.Grain.Id))
                    {
                        result.AddFeedback(new BrokerOperationFeedback($"Grain {item.Grain.Id} was marked for deletion, skipping import", SourceOperationImport, 409, LogLevel.Warning, item.Grain.Id));
                        result.IgnoredCount++;
                        return false;
                    }


                    var existing = await VerifyGrainsExistAsync(null == item.Grain.ParentId ? new[] { item.Grain.Id } : new[] { item.Grain.Id, (Guid)item.Grain.ParentId }, cancellationToken);
                    if (null != item.Grain.ParentId && !existing[(Guid)item.Grain.ParentId])
                    {
                        if (0 == pass)
                        {
                            prevPassFailures++;
                        }
                        else if (pass >= maxRetries)
                        {
                            result.AddFeedback(new BrokerOperationFeedback($"Grain parent {item.Grain.ParentId} doesn't exist", SourceOperationImport, 404, LogLevel.Error, item.Grain.Id));
                        }
                        return true;
                    }

                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Importing grain {id} ({path}, {ctime}), pass {pass}", item.Grain.Id, item.Grain.Path ?? "/", item.Grain.CTime, pass + 1);
                    }

                    var replace = false;
                    IBrokerOperationFeedback? error = null;
                    if (existing[item.Grain.Id])
                    {
                        var ignore = false;
                        if (DuplicatesHandlingStrategy.OverwriteSkipNewer == duplicatesHandling || DuplicatesHandlingStrategy.MergeSkipNewer == duplicatesHandling)
                        {
                            var grainToCheck = await GetGrainAsync(item.Grain.Id, cancellationToken: cancellationToken);
                            if (null != grainToCheck && grainToCheck.MTime >= item.Grain.MTime)
                            {
                                ignore = true;
                            }
                        }

                        switch (duplicatesHandling)
                        {
                            case DuplicatesHandlingStrategy.Ignore:
                                ignore = true;
                                break;
                            case DuplicatesHandlingStrategy.OverwriteSkipNewer:
                            case DuplicatesHandlingStrategy.Overwrite:
                            case DuplicatesHandlingStrategy.OverwriteRecursive:
                                replace = true;
                                break;
                            case DuplicatesHandlingStrategy.MergeSkipNewer:
                            case DuplicatesHandlingStrategy.Merge:
                                if (!ignore)
                                {
                                    error = await ImportGrainAsMerge(item.Grain, pass >= maxRetries, cancellationToken);
                                }
                                break;
                        }
                        if (ignore)
                        {
                            result.IgnoredCount++;
                            result.AddFeedback(error ?? new BrokerOperationFeedback($"Grain {item.Grain.Id} ignored according to duplicates handling strategy", SourceOperationImport, 304, LogLevel.Information, item.Grain.Id));
                            item.Success = true;
                            return true;
                        }
                    }
                    if (replace || !existing[item.Grain.Id])
                    {
                        error = await ImportGrainAsNew(item.Grain, DuplicatesHandlingStrategy.OverwriteRecursive == duplicatesHandling, pass >= maxRetries, cancellationToken);
                    }

                    if (null == error)
                    {
                        result.ImportedCount++;
                        item.Success = true;
                        if (0 < pass)
                        {
                            prevPassFailures--;
                        }
                    }
                    else
                    {
                        if (0 == pass)
                        {
                            prevPassFailures++;
                        }
                        else if (pass >= maxRetries)
                        {
                            result.AddFeedback(error);
                        }
                    }
                    return true;
                }


                foreach (var entry in schemaGrainsByPath)
                {
                    if (!await ImportWorkerFunc(entry.Value))
                    {
                        break;
                    }
                }
                foreach (var entry in otherGrainsByPath)
                {
                    if (!await ImportWorkerFunc(entry.Value))
                    {
                        break;
                    }
                }
                for (var i = 1; i <= maxRetries && 0 < prevPassFailures && !cancellationToken.IsCancellationRequested; i++)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("##### Attempting import pass {pass} on {count} failures (schema: {schemaFails}, other: {otherFails}) #####"
                            , i + 1, prevPassFailures, schemaGrainsByPath.Count(x => !x.Value.Success), otherGrainsByPath.Count(x => !x.Value.Success));
                    }
                    foreach(var entry in schemaGrainsByPath.Where(x => !x.Value.Success))
                    {
                        if (!await ImportWorkerFunc(entry.Value, i))
                        {
                            break;
                        }
                    }
                    foreach (var entry in otherGrainsByPath.Where(x => !x.Value.Success))
                    {
                        if (!await ImportWorkerFunc(entry.Value, i))
                        {
                            break;
                        }
                    }
                }
            }

            if (!cancellationToken.IsCancellationRequested && 0 < deleteFailures.Count)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Final deletion pass on {count} failures", deleteFailures.Count);
                }
                result.DeletedCount += await DeleteGrainsAsync(deleteFailures, cancellationToken);
            }
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Grain import results: imported {imported}, deleted {deleted}, ignored {ignored}, errors {errors}"
                    , result.ImportedCount, result.DeletedCount, result.IgnoredCount, null == result.Feedback ? 0 : result.Feedback.Count(x => LogLevel.Warning < x.FeedbackType));
            }
            return result;
        }
        #endregion

        #region Import Helper Methods

        protected async Task<IBrokerOperationFeedback?> ImportGrainAsMerge(IGrainTransportable grain, bool logErrors = true, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new BrokerOperationFeedback("Operation was cancelled", SourceOperationImport, 204, LogLevel.Warning);
            }
            if (!await VerifyExistingGrainTier(grain, cancellationToken))
            {
                return new BrokerOperationFeedback($"Existiing tier type of grain {grain.Id} is unmergeable", SourceOperationImport, 400, LogLevel.Error);
            }
            try
            {
                IBrokerOperationFeedback? result = null;

                return await WrapInTransaction(result, async (ta) =>
                {
                    if (null != grain.ParentId)
                    {
                        _ = await DisableGrainTimestampTriggers(ta, (Guid)grain.ParentId, cancellationToken);
                    }
                    if (1 > await UpdateOrCreateGrainBaseInTA(ta, grain, cancellationToken))
                    {
                        throw new ApplicationException($"Record for grain {grain.Id} could not be updated");
                    }
                    if (null != grain.ParentId)
                    {
                        _ = await EnableGrainTimestampTriggers(ta, (Guid)grain.ParentId, cancellationToken);
                    }

                    _ = await DisableGrainTimestampTriggers(ta, grain.Id, cancellationToken);

                    if (null != grain.Tier)
                    {
                        switch (true)
                        {
                            case true when grain.Tier is ITypeDef typeDef:
                                _ = await UpdateTypeDefTierInTA(ta, grain.Id, typeDef, cancellationToken);
                                break;
                            case true when grain.Tier is IPropDef propDef:
                                _ = await UpdatePropDefTierInTA(ta, grain.Id, propDef, cancellationToken);
                                break;
                            case true when grain.Tier is IFile file:
                                _ = await UpdateFileTierInTA(ta, grain.Id, file, cancellationToken);
                                break;
                        }
                    }

                    var traitCount = 0;
                    if (true == grain.Traits?.Any())
                    {
                        foreach(var trait in grain.Traits)
                        {
                            traitCount += await StoreImportedTraitInTA(ta, grain.Id, trait, cancellationToken: cancellationToken);
                        }
                    }

                    var langList = grain.Localized.Keys.ToList();
                    var langChecks = (await CheckSystemLanguagesExistAsync(langList, cancellationToken)).ToList();

                    for (var i = 0; i < langList.Count; i++)
                    {
                        var grainLoc = grain.Localized[langList[i]];
                        _ = ImportGrainLanguageAndLabelInTA(ta, grain.Id, langList[i], langChecks[i], grainLoc.Label, cancellationToken);

                        if (true == grainLoc.Traits?.Any())
                        {
                            foreach (var trait in grainLoc.Traits)
                            {
                                traitCount += await StoreImportedTraitInTA(ta, grain.Id, trait, langList[i], cancellationToken);
                            }
                        }
                    }

                    if (0 < traitCount)
                    {
                        _ = await ReindexTraitsInTA(ta, grain, trimOverflow: true, cancellationToken: cancellationToken);
                    }

                    _ = await ImportGrainAclInTA(ta, grain, cancellationToken);
                    _ = await UpdateGrainTimestampsInTA(ta, new[] { grain }, grain.MTime, true, cancellationToken);

                    _ = await EnableGrainTimestampTriggers(ta, grain.Id, cancellationToken);
                    return result;
                }, cancellationToken);
            }
            catch (Exception e)
            {
                if (logErrors && _logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(e, "Error importing {id}", grain.Id);
                }
                return new BrokerOperationFeedback($"Failed to import grain {grain.Id} due to error: {e.Message}", SourceOperationImport, 500, LogLevel.Error, grain.Id);
            }
        }

        protected async Task<IBrokerOperationFeedback?> ImportGrainAsNew(IGrainTransportable grain, bool eraseExistingGrain = false, bool logErrors = true, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new BrokerOperationFeedback("Operation was cancelled", SourceOperationImport, 204, LogLevel.Warning);
            }
            var isBuiltIn = SchemaDefaults.BuiltInIds.Contains(grain.Id);
            var isDeleteable = !isBuiltIn && grain.Tier is not ITypeDef typeDef && grain.Tier is not IPropDef;
            if (!isBuiltIn && null != grain.ParentId && !await _accessService.VerfifyAccessAsync(new[] { (Identifiable)grain.ParentId }, GrainAccessFlag.CreateSubelement, cancellationToken))
            {
                return new BrokerOperationFeedback($"Creating new elements under {grain.ParentId} is prohibited by ACL", SourceOperationImport, 404, LogLevel.Error);
            }
            try
            {
                IBrokerOperationFeedback? result = null;
                return await WrapInTransaction(result, async (ta) =>
                {
                    // whatever fails within this block rolls back entire transaction for the grain
                    if (null != grain.ParentId)
                    {
                        _ = await DisableGrainTimestampTriggers(ta, (Guid)grain.ParentId, cancellationToken);
                    }

                    if (eraseExistingGrain && isDeleteable)
                    {
                        _ = await DeleteGrainsInTA(new[] { grain }, 0, ta, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        _ = await DeleteGrainAclInTA(ta, grain.Id, cancellationToken);
                        _ = await DeleteGrainLabelsInTA(ta, grain.Id, cancellationToken);
                        _ = await DeleteGrainTraitsInTA(ta, grain.Id, true, cancellationToken);
                        foreach (var tierType in new[] { typeof(IPropDef), typeof(IFile) })
                        {
                            if (!tierType.IsAssignableFrom(grain.Tier?.GetType()))
                            {
                                _ = await DeleteGrainTierInTA(ta, grain.Id, tierType, cancellationToken);
                            }
                        }
                    }

                    if (1 > await UpdateOrCreateGrainBaseInTA(ta, grain, cancellationToken))
                    {
                        throw new ApplicationException($"Record for grain {grain.Id} could not be created");
                    }
                    if (null != grain.ParentId)
                    {
                        _ = await EnableGrainTimestampTriggers(ta, (Guid)grain.ParentId, cancellationToken);
                    }

                    _ = await DisableGrainTimestampTriggers(ta, grain.Id, cancellationToken);

                    if (null != grain.Tier)
                    {
                        switch (true)
                        {
                            case true when grain.Tier is ITypeDef typeDef:
                                _ = await CreateTypeDefTierInTA(ta, grain.Id, typeDef, cancellationToken);
                                break;
                            case true when grain.Tier is IPropDef propDef:
                                _ = await CreatePropDefTierInTA(ta, grain.Id, propDef, cancellationToken);
                                break;
                            case true when grain.Tier is IFile file:
                                if (0 == await UpdateFileTierInTA(ta, grain.Id, file, cancellationToken))
                                {
                                    _ = await CreateFileTierInTA(ta, grain.Id, file, cancellationToken);
                                }
                                break;
                        }
                    }

                    async Task TraitCreatorFunc(ITraitTransportable trait, string? lang = null)
                    {
                        trait.Grain = grain;
                        trait.Culture = lang;
                        var traitBase = await CreateTraitInTA(ta, trait, trait.Value, trait.Ord, true, trait.Id, cancellationToken);
                        if (null == traitBase)
                        {
                            throw new ApplicationException($"Trait {trait.Id} for grain {grain.Id} could not be created");
                        }
                    }
                    if (true == grain.Traits?.Any())
                    {
                        foreach (var trait in grain.Traits)
                        {
                            await TraitCreatorFunc(trait);
                        }
                    }

                    var langList = grain.Localized.Keys.ToList();
                    var langChecks = (await CheckSystemLanguagesExistAsync(langList, cancellationToken)).ToList();

                    for (var i = 0; i < langList.Count; i++)
                    {
                        var grainLoc = grain.Localized[langList[i]];
                        _ = await ImportGrainLanguageAndLabelInTA(ta, grain.Id, langList[i], langChecks[i], grainLoc.Label, cancellationToken);

                        if (true == grainLoc.Traits?.Any())
                        {
                            foreach (var trait in grainLoc.Traits)
                            {
                                await TraitCreatorFunc(trait, langList[i]);
                            }
                        }
                    }

                    _ = await ImportGrainAclInTA(ta, grain, cancellationToken);
                    _ = await UpdateGrainTimestampsInTA(ta, new[] { grain }, grain.MTime, true, cancellationToken);

                    _ = await EnableGrainTimestampTriggers(ta, grain.Id, cancellationToken);
                    return result;
                }, cancellationToken);

            }
            catch (Exception e)
            {
                if (logErrors && _logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(e, "Error importing {id}", grain.Id);
                }
                return new BrokerOperationFeedback($"Failed to import grain {grain.Id} due to error: {e.Message}", SourceOperationImport, 500, LogLevel.Error, grain.Id);
            }
        }

        protected async Task<IBrokerOperationFeedback?> ImportDeleteGrain(IIdentifiable grain, CancellationToken cancellationToken = default)
        {
            try
            {
                IBrokerOperationFeedback? result = null;

                return await WrapInTransaction(result, async (ta) =>
                {
                    using (var cmd = ta.Connection!.CreateCommand())
                    {
                        cmd.CommandText = $"{GrainBaseConfig.SQLDelete}{MapGrainBaseColumn(nameof(IGrainBase.Id))} = @{GeneralEntityDefaults.ParamId}";
                        var param = _profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grain.Id);
                        cmd.Parameters.Add(param);

                        _ = await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }
                    return result;
                }, cancellationToken);
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(e, "Error deleting {id}", grain.Id);
                }
                return new BrokerOperationFeedback($"Failed to delete grain {grain.Id} due to error: {e.Message}", SourceOperationImport, 500, LogLevel.Error, grain.Id);
            }
        }

        protected async Task<int> UpdateOrCreateGrainBaseInTA(DbTransaction ta, IGrain sourceGrain, CancellationToken cancellationToken = default)
        {
            var result = 0;
            GrainExtended? targetGrain = null;
            using (var cmd = ta.Connection!.CreateCommand())
            {
                cmd.CommandText = $"{GrainExtendedConfig<TDialect>.SQLSelect}{GeneralEntityDefaults.FieldId} = @{GeneralEntityDefaults.ParamId}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, sourceGrain.Id));

                using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (await rs.ReadAsync(cancellationToken))
                    {
                        targetGrain = new GrainExtended(new GrainExtendedDataAdapter(rs, GrainExtendedDataAdapter.ExtensionColumn.Type | GrainExtendedDataAdapter.ExtensionColumn.Path | GrainExtendedDataAdapter.ExtensionColumn.Container));
                    }
                }
            }
            if (null == targetGrain)
            {
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    cmd.CommandText = $"{GrainBaseConfig.SQLInsert}{PrepareObjectInserParameters<IGrain, GrainExtendedDataAdapter>(cmd.Parameters, sourceGrain)}";
                    result = await cmd.ExecuteNonQueryAsync(cancellationToken);
                }
            }
            else
            {
                var props = typeof(IGrain).GetAllProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Where(x =>
                    x.Name != nameof(IGrain.Id) &&
                    x.Name != nameof(IGrain.ParentId) &&
                    x.Name != nameof(IGrain.TypeDefId) &&
                    true != ((ReadOnlyAttribute?)Attribute.GetCustomAttribute(x, typeof(ReadOnlyAttribute)))?.IsReadOnly);

                foreach (var prop in props)
                {
                    var targetProp = targetGrain.GetType().GetProperty(prop.Name);
                    targetProp?.SetValue(targetGrain, prop.GetValue(sourceGrain));
                }
                targetGrain.Parent = (Identifiable?)sourceGrain.ParentId;
                targetGrain.TypeDef = (Identifiable?)sourceGrain.TypeDefId;

                result = await StoreGrainsInTA(new[] { targetGrain }, 0, ta, true, cancellationToken);
            }
            return result;
        }

        protected async Task<int> StoreImportedTraitInTA(DbTransaction ta, Guid grainId, ITraitTransportable trait, string? lang = null, CancellationToken cancellationToken = default)
        {
            var result = 0;
            using (var cmd = ta.Connection!.CreateCommand())
            {
                var valCol = TraitBaseDataAdapter.GetValueColumn(trait.ValueType);

                trait.Grain = (Identifiable)grainId;
                trait.Culture = lang;
                var insertValsClause = PrepareObjectInserParameters<ITrait, TraitBaseDataAdapter>(cmd.Parameters, trait,
                    new Dictionary<string, (Type, object?)>() { { valCol, (TraitValueFactory.GetValueNativeType(trait.ValueType), trait.Value) } });

                cmd.CommandText = @$"{TraitBaseConfig<TDialect>.SQLInsert} {insertValsClause}
ON CONFLICT({GeneralEntityDefaults.FieldId})
DO UPDATE SET {GeneralEntityDefaults.FieldLangCode} = {EngineSpec<TDialect>.Dialect.ConflictExcluded(GeneralEntityDefaults.FieldLangCode)},
{GeneralEntityDefaults.FieldRevision} = {EngineSpec<TDialect>.Dialect.ConflictExcluded(GeneralEntityDefaults.FieldRevision)},
{TraitBaseDefaults.FieldOrd} = {EngineSpec<TDialect>.Dialect.ConflictExcluded(TraitBaseDefaults.FieldOrd)}, {valCol} = {EngineSpec<TDialect>.Dialect.ConflictExcluded(valCol)}
ON CONFLICT({GeneralEntityDefaults.FieldGrainId}, {TraitBaseDefaults.FieldPropDefId}, {GeneralEntityDefaults.FieldLangCode}, {GeneralEntityDefaults.FieldRevision}, {TraitBaseDefaults.FieldOrd})
DO UPDATE SET {valCol} = {EngineSpec<TDialect>.Dialect.ConflictExcluded(valCol)}";

                result = await cmd.ExecuteNonQueryAsync(cancellationToken);

            }
            return result;
        }

        protected async Task<int> ImportGrainAclInTA(DbTransaction ta, IGrainTransportable grain, CancellationToken cancellationToken = default)
        {
            if (!grain.Acl.Any())
            {
                return 0;
            }
            static string UpdateField(string propName)
            {
                var col = AbstractDataAdapter.GetAdapterColumnName<AclDataAdapter>(propName);
                return $"{col} = {EngineSpec<TDialect>.Dialect.ConflictExcluded(col)}";
            }

            var result = 0;
            using (var cmd = ta.Connection!.CreateCommand())
            {
                cmd.CommandText = $"{AclConfig<TDialect>.SQLInsertAcl}";

                var first = true;
                foreach (var aclentry in grain.Acl)
                {
                    cmd.Parameters.Clear();
                    aclentry.Grain = grain;
                    var insertValsClause = PrepareObjectInserParameters<IAclEntry, AclDataAdapter>(cmd.Parameters, aclentry);
                    if (first)
                    {
                        cmd.CommandText += @$"{insertValsClause}
ON CONFLICT({GeneralEntityDefaults.FieldGrainId}, {AbstractDataAdapter.GetAdapterColumnName<AclDataAdapter>(nameof(IAclEntry.RoleId))})
DO UPDATE SET {UpdateField(nameof(IAclEntry.PermissionMask))}, {UpdateField(nameof(IAclEntry.RestrictionMask))}, {UpdateField(nameof(IAclEntry.Inherit))}";

                        first = false;
                    }
                    result += await cmd.ExecuteNonQueryAsync(cancellationToken);
                }
            }
            return result;
        }

        protected async Task<int> DeleteGrainAclInTA(DbTransaction ta, Guid grainId, CancellationToken cancellationToken = default)
        {
            var result = 0;
            using (var cmd = ta.Connection!.CreateCommand())
            {
                cmd.CommandText = $"{AclConfig<TDialect>.SQLDeleteAcl}{GeneralEntityDefaults.FieldGrainId} = @{GeneralEntityDefaults.ParamGrainId}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, grainId));

                var i = 0;
                var builtInRoles = SchemaDefaults.BuiltInAcl.Aggregate(string.Empty, (aggr, acl) =>
                {
                    if (acl.Item1 == grainId)
                    {
                        if (0 < aggr.Length)
                        {
                            aggr += ",";
                        }
                        var param = _profile.ParameterFactory.Create($"roleId{i++}", acl.Item2);
                        cmd.Parameters.Add(param);
                        aggr += $@"{param.ParameterName}";
                    }
                    return aggr;
                });
                if (!string.IsNullOrEmpty(builtInRoles) && !await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.DeleteBuiltInElements, cancellationToken: cancellationToken))
                {
                    cmd.CommandText += $"AND {AbstractDataAdapter.GetAdapterColumnName<AclDataAdapter>(nameof(IAclEntry.RoleId))} NOT IN ({builtInRoles})";
                }

                result += await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            return result;
        }

        protected async Task<int> DeleteGrainTierInTA(DbTransaction ta, Guid grainId, Type tierType, CancellationToken cancellationToken = default)
        {
            using (var cmd = ta.Connection!.CreateCommand())
            {
                switch (true)
                {
                    case true when typeof(ITypeDef).IsAssignableFrom(tierType):
                        cmd.CommandText = $"{GrainTypeDefConfig<TDialect>.SQLDeleteTypeDef}";
                        break;
                    case true when typeof(IPropDef).IsAssignableFrom(tierType):
                        cmd.CommandText = $"{GrainPropDefConfig<TDialect>.SQLDeletePropDef}";
                        break;
                    case true when typeof(IFile).IsAssignableFrom(tierType):
                        cmd.CommandText = $"{GrainFileConfig<TDialect>.SQLDeleteFile}";
                        break;
                }
                cmd.CommandText += $"{GeneralEntityDefaults.FieldBaseId} = @{GeneralEntityDefaults.ParamId}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grainId));
                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        protected async Task<int> CreateTypeDefTierInTA(DbTransaction ta, Guid grainId, ITypeDef typeDef, CancellationToken cancellationToken = default)
        {
            var result = 0;
            using (var cmd = ta.Connection!.CreateCommand())
            {
                var baseFields = new Dictionary<string, (Type, object?)> { { GeneralEntityDefaults.FieldBaseId, (typeof(Guid), grainId) } };
                var implCol = MapTypeDefColumn(nameof(ITypeDef.Impl));
                cmd.CommandText = @$"{GrainTypeDefConfig<TDialect>.SQLInsertTypeDef}{PrepareObjectInserParameters<ITypeDef, GrainTypeDefDataAdapter>(cmd.Parameters, typeDef, baseFields)}
ON CONFLICT ({GeneralEntityDefaults.FieldBaseId}) DO UPDATE SET {implCol} = {EngineSpec<TDialect>.Dialect.ConflictExcluded(implCol)}";
                result = await cmd.ExecuteNonQueryAsync(cancellationToken);
                if (1 > result)
                {
                    throw new ApplicationException($"TypeDef tier for grain {grainId} could not be created");
                }
            }
            if (typeDef.MixInIds.Any())
            {
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    var idParam = _profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grainId);
                    cmd.Parameters.Add(idParam);

                    cmd.CommandText = $"{GrainTypeDefConfig<TDialect>.SQLDeleteTypeDefMixin}{GrainTypeDefDefaults.MixinExtFieldDerivedType} = @{idParam.ParameterName}";
                    result += await cmd.ExecuteNonQueryAsync(cancellationToken);

                    string rows = PrepareTypeDefMixInInsert(typeDef.MixInIds.Select(x => (Identifiable)x), cmd.Parameters, idParam.ParameterName);
                    cmd.CommandText = $"{GrainTypeDefConfig<TDialect>.SQLInsertTypeDefMixin} ({GrainTypeDefDefaults.MixinExtFieldDerivedType}, {GrainTypeDefDefaults.MixinExtFieldBaseType}) VALUES {rows}";

                    var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                    if (1 > affected)
                    {
                        throw new ApplicationException($"Failed to store mixins for TypeDef {grainId}");
                    }
                    result += affected;
                }
            }
            return result;
        }

        protected async Task<int> UpdateTypeDefTierInTA(DbTransaction ta, Guid grainId, ITypeDef typeDef, CancellationToken cancellationToken = default)
        {
            var grainTypeDef = new GrainTypeDef(grainId, null, null)
            {
                Impl = typeDef.Impl
            };
            foreach (var mixin in typeDef.MixInIds)
            {
                grainTypeDef.AddMixIn((Identifiable)mixin);
            }
            return 0 < grainTypeDef.GetDirtyFields<IGrainTypeDef>().Count
                ? await StoreGrainTypeDefTiersInTA(ta, new[] { grainTypeDef }, cancellationToken: cancellationToken)
                : 0;
        }

        protected async Task<int> CreatePropDefTierInTA(DbTransaction ta, Guid grainId, IPropDef propDef, CancellationToken cancellationToken = default)
        {
            var result = 0;
            using (var cmd = ta.Connection!.CreateCommand())
            {
                var baseFields = new Dictionary<string, (Type, object?)> { { GeneralEntityDefaults.FieldBaseId, (typeof(Guid), grainId) } };
                cmd.CommandText = @$"{GrainPropDefConfig<TDialect>.SQLInsertPropDef}{PrepareObjectInserParameters<IPropDef, GrainPropDefDataAdapter>(cmd.Parameters, propDef, baseFields)}
ON CONFLICT ({GeneralEntityDefaults.FieldBaseId}) DO UPDATE SET ";
                var props = typeof(IPropDef).GetAllProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Where(x =>
                    true != ((ReadOnlyAttribute?)Attribute.GetCustomAttribute(x, typeof(ReadOnlyAttribute)))?.IsReadOnly);
                var first = true;
                foreach (var prop in props)
                {
                    if (!first)
                    {
                        cmd.CommandText += ", ";
                    }
                    var col = MapPropDefColumn(prop.Name);
                    cmd.CommandText += $"{col} = {EngineSpec<TDialect>.Dialect.ConflictExcluded(col)}"; 
                    first = false;
                }

                var valTypeParam = _profile.ParameterFactory.Create($"param{nameof(IValueTypeConstraint.ValueType)}", TraitValueFactory.GetValueTypeAsString(propDef.ValueType));
                if (cmd.Parameters.Contains(valTypeParam.ParameterName))
                {
                    cmd.Parameters.RemoveAt(valTypeParam.ParameterName);
                }
                cmd.Parameters.Add(valTypeParam);

                result = await cmd.ExecuteNonQueryAsync(cancellationToken);
                if (1 > result)
                {
                    throw new ApplicationException($"PropDef tier for grain {grainId} could not be created");
                }
            }
            return result;
        }

        protected async Task<int> UpdatePropDefTierInTA(DbTransaction ta, Guid grainId, IPropDef propDef, CancellationToken cancellationToken = default)
        {
            var grainPropDef = new GrainPropDef(grainId);
            var props = typeof(IPropDef).GetAllProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Where(x => true != ((ReadOnlyAttribute?)Attribute.GetCustomAttribute(x, typeof(ReadOnlyAttribute)))?.IsReadOnly);

            foreach (var prop in props)
            {
                prop.SetValue(grainPropDef, prop.GetValue(propDef));
            }

            return 0 < grainPropDef.GetDirtyFields<IGrainPropDef>().Count
                ? await StoreGrainPropDefTiersInTA(ta, new[] { grainPropDef }, cancellationToken: cancellationToken)
                : 0;
        }

        protected async Task<int> CreateFileTierInTA(DbTransaction ta, Guid grainId, IFile file, CancellationToken cancellationToken = default)
        {
            if (null == file.Content)
            {
                return 0;
            }
            await CreateGrainFileInTA(ta, (Identifiable)grainId, file.Content.Stream, file.MimeType, file.Size, cancellationToken);
            return 1;
        }

        protected async Task<int> UpdateFileTierInTA(DbTransaction ta, Guid grainId, IFile file, CancellationToken cancellationToken = default)
        {
            var grainFile = new GrainFile(grainId, null, null)
            {
                MimeType = file.MimeType,
                Size = file.Size,
                Content = file.Content
            };

            return await StoreGrainFileTiersInTA(ta, new[] { grainFile }, cancellationToken: cancellationToken);
        }

        protected async Task<int> ImportGrainLanguageAndLabelInTA(DbTransaction ta, Guid grainId, string lang, bool langExists = false, string? label = null, CancellationToken cancellationToken = default)
        {
            var result = 0;
            if (!langExists)
            {
                var sysllang = await CreateSystemLanguageInTA(ta, CultureInfo.GetCultureInfo(lang), cancellationToken: cancellationToken);
                if (null == sysllang)
                {
                    throw new ApplicationException($"Language {lang} could not be created");
                }
                result++;
            }
            if (!string.IsNullOrEmpty(label))
            {
                result += await StoreGrainLabelInTA(ta, grainId, lang, label, cancellationToken);
            }
            return result;
        }

        protected async Task<int> UpdateGrainTimestampsInTA(DbTransaction ta, IEnumerable<IIdentifiable> grains, DateTime? timestamp = null, bool aclWasChecked = false, CancellationToken cancellationToken = default)
        {
            if (!aclWasChecked && !await _accessService.VerfifyAccessAsync(grains, GrainAccessFlag.Write, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.Write);
            }
            var result = 0;
            using (var cmd = ta.Connection!.CreateCommand())
            {
                var vals = grains.Select((x, index) =>
                {
                    var paramName = $"{GeneralEntityDefaults.ParamId}{index}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(paramName, x.Id));
                    return paramName;
                });
                var mtimeCol = MapGrainBaseColumn(nameof(IGrain.MTime));
                cmd.CommandText = $"{GrainBaseConfig.SQLUpdate}{mtimeCol} = @{GrainBaseConfig.ParamMTime} WHERE {GeneralEntityDefaults.FieldId} IN (@{string.Join(",@", vals)}) AND {mtimeCol} <> @{GrainBaseConfig.ParamMTime}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainBaseConfig.ParamMTime, timestamp ?? DateTime.UtcNow));

                result = await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            return result;
        }
        #endregion

        #region Export Helper Methods
        protected async Task<IGrainTierTypeDef?> GetGrainTierTypeDef(IIdentifiable grain, CancellationToken cancellationToken = default)
        {
            GrainTierTypeDef? result = null;
            await ExecuteOnConnection(result, async (cmd) =>
            {
                using (cmd)
                {
                    cmd.CommandText = $"{GrainTypeDefConfig<TDialect>.SQLSelectTypeDefTier}{GeneralEntityDefaults.FieldBaseId} = @{GeneralEntityDefaults.ParamId}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grain.Id));

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            result = new(new GrainTypeDefDataAdapter(rs));
                        }
                    }
                }
                return result;
            }, cancellationToken);

            if (null != result)
            {
                result.MixInIds = await GetTypeDefMixedInTypeIds(grain.Id, cancellationToken);
            }
            return result;
        }

        protected async Task<IGrainTierPropDef?> GetGrainTierPropDef(IIdentifiable grain, CancellationToken cancellationToken = default)
        {
            GrainTierPropDef? result = null;
            return await ExecuteOnConnection(result, async (cmd) =>
            {
                using (cmd)
                {
                    cmd.CommandText = $"{GrainPropDefConfig<TDialect>.SQLSelectPropDefTier}{GeneralEntityDefaults.FieldBaseId} = @{GeneralEntityDefaults.ParamId}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grain.Id));

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            result = new(new GrainPropDefDataAdapter(rs));
                        }
                    }
                }
                return result;
            }, cancellationToken);
        }

        protected async Task<IGrainTierFile?> GetGrainTierFile(IIdentifiable grain, CancellationToken cancellationToken = default)
        {
            GrainTierFile? result = null;
            return await ExecuteOnConnection(result, async (cmd) =>
            {
                using (cmd)
                {
                    cmd.CommandText = $"{GrainFileConfig<TDialect>.SQLSelectFileTier}{GeneralEntityDefaults.FieldBaseId} = @{GeneralEntityDefaults.ParamId}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grain.Id));

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            result = new(CreateFileAdapter(rs, GrainFileContentAccess.Immediate), true);
                        }
                    }
                }

                return result;
            }, cancellationToken);
        }

        protected async Task<IEnumerable<IAclEntryTransportable>> GetGrainAclTransportable(IIdentifiable grain, CancellationToken cancellationToken = default)
        {
            var result = new List<IAclEntryTransportable>();
            return await ExecuteOnConnection(result, async (cmd) =>
            {
                using (cmd)
                {
                    cmd.CommandText = $"{AclConfig<TDialect>.SQLSelectAcl}{GeneralEntityDefaults.FieldGrainId} = @{GeneralEntityDefaults.ParamGrainId} ORDER BY {MapAclColumn(nameof(ISchemaAclEntry.RoleId))}, {MapAclColumn(nameof(ISchemaAclEntry.Inherit))}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, grain.Id));

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await rs.ReadAsync(cancellationToken))
                        {
                            result.Add(new AclEntryTransportable(new AclDataAdapter(rs, AclDataAdapter.ExtensionColumn.None)));
                        }
                    }
                }
                return result;
            }, cancellationToken);
        }

        protected async Task<IDictionary<string, IGrainLocalizedLayer>> GetGrainLocalizedLayers(IGrain grain, IDictionary<string, IEnumerable<ITraitTransportable>> traits, CancellationToken cancellationToken = default)
        {
            var result = new SortedDictionary<string, IGrainLocalizedLayer>();
            IGrainLocalizedLayer GetLayer(string lang)
            {
                IGrainLocalizedLayer layer;
                if (result.ContainsKey(lang))
                {
                    layer = result[lang];
                }
                else
                {
                    layer = result[lang] = new GrainLocalizedLayer();
                }
                return layer;
            }
            
            await ExecuteOnConnection(0, async (cmd) =>
            {
                using (cmd)
                {
                    cmd.CommandText = $"{GrainLocalizedConfig<TDialect>.SQLSelectLabel}{GeneralEntityDefaults.FieldGrainId} = @{GeneralEntityDefaults.ParamGrainId}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, grain.Id));

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        var langOrd = rs.GetOrdinal(GeneralEntityDefaults.FieldLangCode);
                        var labelOrd = rs.GetOrdinal(GrainLocalizedDefaults.FieldLabel);

                        while (await rs.ReadAsync(cancellationToken))
                        {
                            var lang = rs.GetString(langOrd);

                            GetLayer(lang).Label = rs.GetString(labelOrd);
                        }
                    }
                }
                return 0;
            }, cancellationToken);

            foreach (var entry in traits)
            {
                if (!string.IsNullOrEmpty(entry.Key) && entry.Value.Any())
                {
                    GetLayer(entry.Key).Traits = entry.Value;
                }
            }
            return result;
        }

        protected async Task<IDictionary<string, IEnumerable<ITraitTransportable>>> GetGrainTraitsTransportable(IIdentifiable grain, CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<string, IEnumerable<ITraitTransportable>>();
            return await ExecuteOnConnection(result, async (cmd) =>
            {
                cmd.CommandText = @$"{TraitBaseConfig<TDialect>.SQLSelect}{GeneralEntityDefaults.FieldGrainId} = @{GeneralEntityDefaults.ParamGrainId}
ORDER BY {GeneralEntityDefaults.FieldLangCode}, {MapTraitColumn(nameof(ITraitBase.Revision))}, {MapTraitColumn(nameof(ITraitBase.PropDefId))}, {MapTraitColumn(nameof(ITraitBase.Ord))}";

                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, grain.Id));

                using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    while (await rs.ReadAsync(cancellationToken))
                    {
                        var traitBase = ReadTrait(rs);

                        var langKey = traitBase.Culture ?? string.Empty;
                        IList<ITraitTransportable> traits;
                        if (result.TryGetValue(langKey, out IEnumerable<ITraitTransportable>? values))
                        {
                            traits = (IList<ITraitTransportable>)values;
                        }
                        else
                        {
                            traits = new List<ITraitTransportable>();
                            result[langKey] = traits;
                        }

                        switch (traitBase.ValueType)
                        {
                            case TraitValueType.Memo:
                                traits.Add(new TraitTransportableMemo(traitBase));
                                break;
                            case TraitValueType.Number:
                                traits.Add(new TraitTransportableNumber(traitBase));
                                break;
                            case TraitValueType.Boolean:
                                traits.Add(new TraitTransportableBoolean(traitBase));
                                break;
                            case TraitValueType.DateTime:
                                traits.Add(new TraitTransportableDateTime(traitBase));
                                break;
                            case TraitValueType.Grain:
                                traits.Add(new TraitTransportableGrain(traitBase));
                                break;
                            case TraitValueType.File:
                                traits.Add(new TraitTransportableFile(traitBase));
                                break;
                            default:
                                traits.Add(new TraitTransportableText(traitBase));
                                break;
                        }
                    }
                }
                return result;
            }, cancellationToken);
        }
        #endregion

        #region Common Helper Methods
        protected async Task<bool> VerifyExistingGrainTier(IGrainTransportable grain, CancellationToken cancellationToken = default)
        {
            var result = false;
            return await ExecuteOnConnection(result, async (cmd) =>
            {
                using (cmd)
                {
                    var param = _profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grain.Id);
                    cmd.Parameters.Add(param);

                    cmd.CommandText = $"SELECT COUNT(*) FROM ";
                    if (null == grain.Tier)
                    {
                        cmd.CommandText += "(";
                        var first = true;
                        foreach (var tbl in new[] { GrainTypeDefDefaults.DataSourceTypeDef, GrainPropDefDefaults.DataSourcePropDef, GrainFileDefaults.DataSourceFile })
                        {
                            if (!first)
                            {
                                cmd.CommandText += " UNION ";
                            }
                            cmd.CommandText += $"SELECT {GeneralEntityDefaults.FieldBaseId} FROM {tbl} WHERE {GeneralEntityDefaults.FieldBaseId} = @{param.ParameterName}";
                            first = false;
                        }
                        cmd.CommandText += ")";
                    }
                    else
                    {
                        switch (true)
                        {
                            case true when grain.Tier is ITypeDef:
                                cmd.CommandText += GrainTypeDefDefaults.DataSourceTypeDef;
                                break;
                            case true when grain.Tier is IPropDef:
                                cmd.CommandText += GrainPropDefDefaults.DataSourcePropDef;
                                break;
                            case true when grain.Tier is IFile:
                                cmd.CommandText += GrainFileDefaults.DataSourceFile;
                                break;
                        }
                        cmd.CommandText += $" WHERE {GeneralEntityDefaults.FieldBaseId} = @{param.ParameterName}";
                    }

                    var count = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
                    result = null == grain.Tier ? (0 == count) : (1 == count);
                }
                return result;
            }, cancellationToken);
        }
        #endregion
    }
}
