using CraftedSolutions.MarBasBrokerSQLCommon.Grain;
using CraftedSolutions.MarBasBrokerSQLCommon.GrainDef;
using CraftedSolutions.MarBasBrokerSQLCommon.GrainTier;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Broker;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.GrainDef;
using CraftedSolutions.MarBasSchema.GrainTier;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace CraftedSolutions.MarBasBrokerSQLCommon.BrokerImpl
{
    public abstract class GrainManagementBroker<TDialect>
        : SystemManagementBroker<TDialect>, IGrainManagementBroker, IAsyncGrainManagementBroker
         where TDialect : ISQLDialect, new()
    {
        protected GrainManagementBroker(IBrokerProfile profile, ILogger logger) : base(profile, logger)
        {
        }

        protected GrainManagementBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger logger) : base(profile, context, accessService, logger)
        {
        }

        public IGrainLocalized? GetGrain(Guid id, CultureInfo? culture = null)
        {
            return GetGrainAsync(id, culture).Result;
        }

        public async Task<IGrainLocalized?> GetGrainAsync(Guid id, CultureInfo? culture = null, CancellationToken cancellationToken = default)
        {
            await CheckProfile(cancellationToken);
            return await ExecuteOnConnection<IGrainLocalized?>(null, async (cmd) =>
            {
                using (cmd)
                {
                    cmd.CommandText = $"{GrainLocalizedConfig<TDialect>.SQLSelectByAclLocalized}g.{MapGrainBaseColumn(nameof(IGrainBase.Id))} = @{GeneralEntityDefaults.ParamId}";
                    _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, (await _accessService.GetContextPrimaryRoleAsync(cancellationToken)).Id);
                    _profile.ParameterFactory.AddParametersForCultureLayer(cmd.Parameters, culture);
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, id));
                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("GetGrainAsync id={id}, HasRows={hasRows}", id.ToString("D"), rs.HasRows);
                        }
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            return new GrainLocalized(new GrainLocalizedDataAdapter(rs));
                        }
                    }
                }
                return null;
            }, cancellationToken);
        }

        public IGrainBase? CreateGrain(string name, IIdentifiable parent, IIdentifiable? typedef)
        {
            return CreateGrainAsync(name, parent, typedef).Result;
        }

        public async Task<IGrainBase?> CreateGrainAsync(string name, IIdentifiable parent, IIdentifiable? typedef, CancellationToken cancellationToken = default)
        {
            await CheckProfile(cancellationToken);
            IGrainBase? result = null;
            return await WrapInTransaction(result, async (ta) =>
            {
                return await CreateGrainInTA(name, parent, typedef, ta, cancellationToken: cancellationToken);
            }, cancellationToken);
        }

        public int DeleteGrains(IEnumerable<IIdentifiable> ids)
        {
            return DeleteGrainsAsync(ids).Result;
        }

        public async Task<int> DeleteGrainsAsync(IEnumerable<IIdentifiable> ids, CancellationToken cancellationToken = default)
        {
            await CheckProfile(cancellationToken);
            var result = 0;
            await WrapInTransaction(result, async (ta) =>
            {
                return await DeleteGrainsInTA(ids, result, ta, cancellationToken: cancellationToken);
            }, cancellationToken);
            return result;
        }

        public int StoreGrains(IEnumerable<IGrainBase> grains)
        {
            return StoreGrainsAsync(grains).Result;
        }

        public async Task<int> StoreGrainsAsync(IEnumerable<IGrainBase> grains, CancellationToken cancellationToken = default)
        {
            var grainsMod = grains.Where(g => 0 < g.GetDirtyFields<IGrainBase>().Count);
            var grainsModL = grains.Where(g => g is IGrainLocalized gl && 0 < gl.GetDirtyFields<IGrainLocalized>().Count).Select(g => (IGrainLocalized)g);
            if (!grainsMod.Any() && !grainsModL.Any())
            {
                return -1;
            }
            await CheckProfile(cancellationToken);
            var result = 0;
            return await WrapInTransaction(result, async (ta) =>
            {
                result = await StoreGrainsInTA(grainsMod, result, ta, false, cancellationToken);
                return await StoreLocalizedGrainsInTA(grainsModL, result, ta, cancellationToken);
            }, cancellationToken);
        }

        public IGrainBase? MoveGrain(IIdentifiable grain, IIdentifiable newParent)
        {
            return MoveGrainAsync(grain, newParent).Result;
        }

        public async Task<IGrainBase?> MoveGrainAsync(IIdentifiable grain, IIdentifiable newParent, CancellationToken cancellationToken = default)
        {
            await CheckProfile(cancellationToken);
            if (!await _accessService.VerfifyAccessAsync(new[] { grain }, GrainAccessFlag.Read, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.Read);
            }
            if (!await _accessService.VerfifyAccessAsync(new[] { grain }, GrainAccessFlag.Delete, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.Delete);
            }
            if (!await _accessService.VerfifyAccessAsync(new[] { newParent }, GrainAccessFlag.CreateSubelement, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.CreateSubelement);
            }
            IGrainBase? result = null;
            await WrapInTransaction(result, async (ta) =>
            {
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    cmd.CommandText = $"{GrainBaseConfig.SQLUpdate}{MapGrainBaseColumn(nameof(IGrainBase.ParentId))} = @{GrainBaseConfig.ParamParentId} WHERE {GeneralEntityDefaults.FieldId} = @{GeneralEntityDefaults.ParamId}{EngineSpec<TDialect>.Dialect.ReturnFromInsert}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainBaseConfig.ParamParentId, newParent.Id));
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grain.Id));

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            result = new GrainBase(new GrainExtendedDataAdapter(rs, GrainExtendedDataAdapter.ExtensionColumn.None));
                        }
                    }
                }
                return result;
            }, cancellationToken);
            return result;
        }

        public bool IsGrainInstanceOf(IIdentifiable grain, IIdentifiable typedef)
        {
            return IsGrainInstanceOfAsync(grain, typedef).Result;
        }

        public async Task<bool> IsGrainInstanceOfAsync(IIdentifiable grain, IIdentifiable typedef, CancellationToken cancellationToken = default)
        {
            await CheckProfile(cancellationToken);
            Guid? typeDefId = null;
            if (grain is IGrainBase grainBase)
            {
                typeDefId = grainBase.TypeDefId ?? SchemaDefaults.TypeDefTypeDefID;
            }
            if (null != typeDefId && typeDefId.Equals(typedef.Id))
            {
                return true;
            }

            return await ExecuteOnConnection(false, async (cmd) =>
            {
                using (cmd)
                {
                    cmd.CommandText = $"{GrainTypeDefConfig<TDialect>.SQLSelectTypeDefMixinAnc}{GrainTypeDefDefaults.MixinExtFieldStart} = ";
                    if (null == typeDefId)
                    {
                        cmd.CommandText = $"{GrainTypeDefConfig<TDialect>.SQLSelectTypeDefMixinDefault}{cmd.CommandText}";
                        cmd.CommandText += $"({GrainBaseConfig.SQLSelectTypeDef}{MapGrainBaseColumn(nameof(IGrainBase.Id))} = @{GeneralEntityDefaults.ParamId})";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grain.Id));
                    }
                    else
                    {
                        cmd.CommandText += $"@{GrainTypeDefDefaults.ParamTypeDefId}";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainTypeDefDefaults.ParamTypeDefId, typeDefId));
                    }

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        var ordBase = rs.GetOrdinal(GrainTypeDefDefaults.MixinExtFieldBaseType);
                        var ordStart = rs.GetOrdinal(GrainTypeDefDefaults.MixinExtFieldStart);
                        while (await rs.ReadAsync(cancellationToken))
                        {
                            if (!rs.IsDBNull(ordStart) && rs.GetGuid(ordStart).Equals(typedef.Id) || !rs.IsDBNull(ordBase) && rs.GetGuid(ordBase).Equals(typedef.Id))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }, cancellationToken);
        }

        public Type? GetGrainTier(IIdentifiable grain)
        {
            return GetGrainTierAsync(grain).Result;
        }

        public async Task<Type?> GetGrainTierAsync(IIdentifiable grain, CancellationToken cancellationToken = default)
        {
            foreach (var type in SchemaDefaults.GrainTierTypes)
            {
                if (type.IsAssignableFrom(grain.GetType()))
                {
                    return type;
                }
            }
            Type? result = null;
            return await ExecuteOnConnection(result, async (cmd) =>
            {
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grain.Id));
                cmd.CommandText = SchemaDefaults.GrainTierTypes.Aggregate(string.Empty, (aggr, type) =>
                {
                    var tbl = string.Empty;
                    switch (true)
                    {
                        case true when type == typeof(IFile):
                            tbl = GrainFileDefaults.DataSourceFile;
                            break;
                        case true when type == typeof(ITypeDef):
                            tbl = GrainTypeDefDefaults.DataSourceTypeDef;
                            break;
                        case true when type == typeof(IPropDef):
                            tbl = GrainPropDefDefaults.DataSourcePropDef;
                            break;
                    }
                    if (0 < aggr.Length)
                    {
                        aggr += " UNION ";
                    }
                    aggr += $"SELECT '{type.AssemblyQualifiedName}' AS tier WHERE EXISTS (SELECT {GeneralEntityDefaults.FieldBaseId} FROM {tbl} WHERE {GeneralEntityDefaults.FieldBaseId} = @{GeneralEntityDefaults.ParamId})";
                    return aggr;
                });

                var tier = await cmd.ExecuteScalarAsync(cancellationToken);
                if (null != tier)
                {
                    result = Type.GetType(tier.ToString()!);
                }
                return result;
            }, cancellationToken);
        }

        public IEnumerable<IGrainLocalized> ListGrains(IIdentifiable? container, bool recursive = false, CultureInfo? culture = null, IEnumerable<IListSortOption<GrainSortField>>? sortOptions = null, IGrainQueryFilter? filter = null)
        {

            return ListGrainsAsync(container, recursive, culture, sortOptions, filter).Result;
        }

        public async Task<IEnumerable<IGrainLocalized>> ListGrainsAsync(IIdentifiable? container, bool recursive = false, CultureInfo? culture = null, IEnumerable<IListSortOption<GrainSortField>>? sortOptions = null, IGrainQueryFilter? filter = null, CancellationToken cancellationToken = default)
        {
            await CheckProfile(cancellationToken);
            return await ExecuteOnConnection<IEnumerable<IGrainLocalized>>(Enumerable.Empty<IGrainLocalized>(), async (cmd) =>
            {
                using (cmd)
                {
                    var parentId = container?.Id ?? SchemaDefaults.RootID;
                    cmd.CommandText = recursive && SchemaDefaults.RootID.Equals(parentId)
                        ? GrainLocalizedConfig<TDialect>.SQLSelectByAclLocalized
                        : $"{GrainLocalizedConfig<TDialect>.SQLSelectByAclLocalized}{(recursive ? $"{GrainBaseConfig.GrainExtFieldIdPath} LIKE" : $"{MapGrainBaseColumn(nameof(IGrainBase.ParentId))} =")} @{GrainBaseConfig.ParamParentId}";

                    _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, (await _accessService.GetContextPrimaryRoleAsync(cancellationToken)).Id);
                    _profile.ParameterFactory.AddParametersForCultureLayer(cmd.Parameters, culture);
                    if (recursive)
                    {
                        if (SchemaDefaults.RootID.Equals(parentId))
                        {
                            var param = _profile.ParameterFactory.Create("fantomRoot", SchemaDefaults.AnyGrainID);
                            cmd.Parameters.Add(param);
                            cmd.CommandText += $"(g.{GeneralEntityDefaults.FieldId} <> @{param.ParameterName})";
                        }
                        else
                        {
                            cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainBaseConfig.ParamParentId, $"%{parentId}/%"));
                        }
                    }
                    else
                    {
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainBaseConfig.ParamParentId, parentId));
                    }

                    cmd.CommandText += PrepareGrainQueryFilterParameters(cmd.Parameters, filter, "g");
                    cmd.CommandText += PrepareListOrderByClause<GrainSortField, GrainLocalizedDataAdapter>(sortOptions, "g");

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("ListGrainsAsync containerId={containeId}, recursive={recursive}, HasRows={hasRows}",
                                cmd.Parameters[0].Value, recursive, rs.HasRows);
                        }
                        return await EnumGrainDataReader<IGrainLocalized, GrainLocalized, GrainLocalizedDataAdapter>(rs, GrainExtendedDataAdapter.ExtensionColumn.All, cancellationToken).ToListAsync(cancellationToken);
                    }
                }
            }, cancellationToken);
        }

        public IEnumerable<IGrainLocalized> ResolvePath(string? path, CultureInfo? culture = null, IEnumerable<IListSortOption<GrainSortField>>? sortOptions = null, IGrainQueryFilter? filter = null)
        {
            return ResolvePathAsync(path, culture, sortOptions, filter).Result;
        }

        public async Task<IEnumerable<IGrainLocalized>> ResolvePathAsync(string? path, CultureInfo? culture = null, IEnumerable<IListSortOption<GrainSortField>>? sortOptions = null, IGrainQueryFilter? filter = null, CancellationToken cancellationToken = default)
        {
            await CheckProfile(cancellationToken);
            if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
            {
                var root = await GetGrainAsync(SchemaDefaults.RootID, culture, cancellationToken);
                return new IGrainLocalized[] { root! };
            }
            if ("*".Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                return await ListGrainsAsync(null, false, culture, sortOptions, filter, cancellationToken);
            }
            if ("**".Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                return await ListGrainsAsync(null, true, culture, sortOptions, filter, cancellationToken);
            }
            return await ExecuteOnConnection<IEnumerable<IGrainLocalized>>(Enumerable.Empty<IGrainLocalized>(), async (cmd) =>
            {
                using (cmd)
                {
                    var pathCol = MapGrainBaseColumn(nameof(IGrainBase.Path));
                    var pathParam = "@path";
                    var roleId = (await _accessService.GetContextPrimaryRoleAsync(cancellationToken)).Id;
                    if (path!.EndsWith("/**", StringComparison.OrdinalIgnoreCase))
                    {
                        cmd.CommandText = $"{GrainLocalizedConfig<TDialect>.SQLSelectByAclLocalized}{pathCol} LIKE {pathParam}";
                        _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, roleId);
                        _profile.ParameterFactory.AddParametersForCultureLayer(cmd.Parameters, culture);
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(pathParam, $"{SchemaDefaults.RootName}/{path.TrimEnd('/', '*')}/%"));
                    }
                    else if (path.EndsWith("/*", StringComparison.OrdinalIgnoreCase))
                    {
                        cmd.CommandText = $"{GrainLocalizedConfig<TDialect>.SQLSelectByAclLocalized}{MapGrainBaseColumn(nameof(IGrainBase.ParentId))} = (SELECT {MapGrainBaseColumn(nameof(IGrainBase.Id))} FROM {GrainBaseConfig.DataSourceExt} WHERE {pathCol} = {pathParam})";
                        _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, roleId);
                        _profile.ParameterFactory.AddParametersForCultureLayer(cmd.Parameters, culture);
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(pathParam, $"{SchemaDefaults.RootName}/{path.TrimEnd('/', '*')}"));
                    }
                    else
                    {
                        cmd.CommandText = $"{GrainLocalizedConfig<TDialect>.SQLSelectByAclLocalized}{pathCol} = {pathParam}";
                        _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, roleId);
                        _profile.ParameterFactory.AddParametersForCultureLayer(cmd.Parameters, culture);
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(pathParam, $"{SchemaDefaults.RootName}/{path.TrimEnd('/')}"));
                    }

                    cmd.CommandText += PrepareGrainQueryFilterParameters(cmd.Parameters, filter, "g");
                    cmd.CommandText += PrepareListOrderByClause<GrainSortField, GrainLocalizedDataAdapter>(sortOptions, "g");

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        return await EnumGrainDataReader<IGrainLocalized, GrainLocalized, GrainLocalizedDataAdapter>(rs, GrainExtendedDataAdapter.ExtensionColumn.All, cancellationToken).ToListAsync(cancellationToken);
                    }
                }
            }, cancellationToken);
        }

        public IEnumerable<IGrainLocalized> GetGrainAncestors(IIdentifiable grain, CultureInfo? culture = null, bool includeSelf = false)
        {
            return GetGrainAncestorsAsync(grain, culture, includeSelf).Result;
        }

        public async Task<IEnumerable<IGrainLocalized>> GetGrainAncestorsAsync(IIdentifiable grain, CultureInfo? culture = null, bool includeSelf = false, CancellationToken cancellationToken = default)
        {
            await CheckProfile(cancellationToken);
            return await ExecuteOnConnection<IEnumerable<IGrainLocalized>>(Enumerable.Empty<IGrainLocalized>(), async (cmd) =>
            {
                using (cmd)
                {
                    cmd.CommandText = $"{GrainLocalizedConfig<TDialect>.SQLSelectPathByAclLocalized}a.{GrainBaseConfig.PathFieldStart} = @{GrainBaseConfig.PathParamStart}";
                    if (!includeSelf)
                    {
                        cmd.CommandText += $" AND a.{GrainBaseConfig.PathFieldDistance} > 0";
                    }
                    cmd.CommandText += $" ORDER BY a.{GrainBaseConfig.PathFieldStart}, a.{GrainBaseConfig.PathFieldDistance}";

                    _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, (await _accessService.GetContextPrimaryRoleAsync(cancellationToken)).Id);
                    _profile.ParameterFactory.AddParametersForCultureLayer(cmd.Parameters, culture);
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainBaseConfig.PathParamStart, grain.Id));

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        return await EnumGrainDataReader<IGrainLocalized, GrainLocalized, GrainLocalizedDataAdapter>(rs, GrainExtendedDataAdapter.ExtensionColumn.All, cancellationToken).ToListAsync(cancellationToken);
                    }
                }
            }, cancellationToken);
        }

        public IDictionary<Guid, bool> VerifyGrainsExist(IEnumerable<Guid> ids)
        {
            return VerifyGrainsExistAsync(ids).Result;
        }

        public async Task<IDictionary<Guid, bool>> VerifyGrainsExistAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<Guid, bool>(ids.Select(x => new KeyValuePair<Guid, bool>(x, false)));
            if (!ids.Any())
            {
                return result;
            }
            await CheckProfile(cancellationToken);
            return await ExecuteOnConnection(result, async (cmd) =>
            {
                using (cmd)
                {
                    var idsOrder = ids.ToList();

                    var vals = ids.Select((x, index) =>
                    {
                        var paramName = $"{GeneralEntityDefaults.ParamId}{index}";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(paramName, x));
                        return paramName;
                    });
                    cmd.CommandText = $"SELECT {GeneralEntityDefaults.FieldId} FROM {GrainBaseConfig.DataSource} WHERE {GeneralEntityDefaults.FieldId} IN (@{string.Join(",@", vals)})";

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await rs.ReadAsync(cancellationToken))
                        {
                            result[rs.GetGuid(0)] = true;
                        }
                    }
                }
                return result;
            }, cancellationToken);
        }

        public IEnumerable<IGrainLabel> GetGrainLabels(IEnumerable<Guid> grainIds, IEnumerable<CultureInfo>? cultures = null)
        {
            return GetGrainLabelsAsync(grainIds, cultures).Result;
        }

        public async Task<IEnumerable<IGrainLabel>> GetGrainLabelsAsync(IEnumerable<Guid> grainIds, IEnumerable<CultureInfo>? cultures = null, CancellationToken cancellationToken = default)
        {
            if (grainIds?.Any() != true)
            {
                return Enumerable.Empty<IGrainLabel>();
            }
            await CheckProfile(cancellationToken);
            return await ExecuteOnConnection<IEnumerable<IGrainLabel>>(Enumerable.Empty<IGrainLabel>(), async (cmd) =>
            {
                var i = 0;
                var grainIdClause = $"IN ({grainIds.Aggregate(string.Empty, (aggr, id) =>
                {
                    var result = aggr;
                    if (0 < result.Length)
                    {
                        result += ", ";
                    }
                    var paramName = $"{GeneralEntityDefaults.ParamGrainId}{i++}";
                    result += $"@{paramName}";

                    cmd.Parameters.Add(_profile.ParameterFactory.Create(paramName, id));
                    return result;
                })})";

                if (cultures?.Count() == 1)
                {
                    cmd.CommandText =
$@"SELECT g.{GeneralEntityDefaults.FieldId} AS {GeneralEntityDefaults.FieldGrainId}, l.{GeneralEntityDefaults.FieldLangCode}, COALESCE(l.{GrainLocalizedDefaults.FieldLabel}, g.{MapGrainBaseColumn(nameof(INamed.Name))}) AS {GrainLocalizedDefaults.FieldLabel}
FROM {GrainBaseConfig.DataSource} AS g
{GrainLocalizedConfig<TDialect>.SQLJoinLabel}
WHERE g.{GeneralEntityDefaults.FieldId} {grainIdClause}";
                    _profile.ParameterFactory.AddParametersForCultureLayer(cmd.Parameters, cultures.FirstOrDefault());
                }
                else
                {
                    cmd.CommandText = $"SELECT l.* FROM {GrainLocalizedDefaults.DataSourceLabel} AS l WHERE l.{GeneralEntityDefaults.FieldGrainId} {grainIdClause}";

                    if (cultures?.Any() == true)
                    {
                        i = 0;
                        cmd.CommandText += $" AND ({cultures.Aggregate(string.Empty, (aggr, culture) =>
                        {
                            var result = aggr;
                            if (0 < result.Length)
                            {
                                result += " OR ";
                            }

                            var paramName = $"{GeneralEntityDefaults.ParamLangCode}{i}";
                            cmd.Parameters.Add(_profile.ParameterFactory.Create(paramName, culture.IetfLanguageTag));
                            result += $"l.{GeneralEntityDefaults.FieldLangCode} = @{paramName}";

                            i++;
                            return result;
                        })})";
                    }
                    cmd.CommandText += $" ORDER BY {GeneralEntityDefaults.FieldGrainId}, {GeneralEntityDefaults.FieldLangCode}";
                }

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("GetGrainLabelsAsync: {sql}", cmd.CommandText);
                }

                using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    var result = new List<IGrainLabel>();
                    while (await rs.ReadAsync(cancellationToken))
                    {
                        result.Add(new GrainLabel(new GrainLabelAdapter(rs)));
                    }
                    return result;
                }
            }, cancellationToken);
        }

        #region Helper Methods
        protected async Task<IGrainBase?> CastIdentifiableToGrainBase(IIdentifiable grain, CancellationToken cancellationToken)
        {
            IGrainBase? result = grain as IGrainBase;
            if (null == result)
            {
                result = await ExecuteOnConnection(result, async (cmd) =>
                {
                    using (cmd)
                    {
                        cmd.CommandText = $"{GrainBaseConfig.SQLSelect}{GeneralEntityDefaults.FieldId} = @{GeneralEntityDefaults.ParamId}";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grain.Id));
                        using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                        {
                            if (await rs.ReadAsync(cancellationToken))
                            {
                                return new GrainBase(new GrainExtendedDataAdapter(rs, GrainExtendedDataAdapter.ExtensionColumn.Type | GrainExtendedDataAdapter.ExtensionColumn.Path));
                            }
                        }
                    }
                    return result;
                }, cancellationToken);
            }
            return result;
        }

        protected async Task<IGrainBase?> CreateGrainInTA(string name, IIdentifiable parent, IIdentifiable? typedef, DbTransaction ta, bool aclWasChecked = false, Guid? newId = null, CancellationToken cancellationToken = default)
        {
            if (!aclWasChecked && !await _accessService.VerfifyAccessAsync(new[] { parent }, GrainAccessFlag.CreateSubelement, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.CreateSubelement);
            }
            IGrainBase? result = null;
            using (var cmd = ta.Connection!.CreateCommand())
            {
                var cols = new List<string>() {
                            MapGrainBaseColumn(nameof(IGrainBase.Id)),
                            MapGrainBaseColumn(nameof(IGrainBase.ParentId)),
                            MapGrainBaseColumn(nameof(IGrainBase.Name)),
                            MapGrainBaseColumn(nameof(IGrainBase.Owner))
                        };
                var vals = new List<string>() { GrainBaseConfig.ParamParentId, GrainBaseConfig.ParamName, GrainBaseConfig.ParamOwner };
                if (null != typedef)
                {
                    cols.Add(MapGrainBaseColumn(nameof(IGrainBase.TypeDefId)));
                    vals.Add(GrainTypeDefDefaults.ParamTypeDefId);
                }
                cmd.CommandText = $"{GrainBaseConfig.SQLInsert}({string.Join(",", cols)}) VALUES ({(null == newId ? EngineSpec<TDialect>.Dialect.GuidGen : $"@{GeneralEntityDefaults.ParamId}")}, @{string.Join(",@", vals)}){EngineSpec<TDialect>.Dialect.ReturnFromInsert}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainBaseConfig.ParamParentId, parent.Id));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainBaseConfig.ParamName, GrainBase.SanitizeName(name)));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainBaseConfig.ParamOwner, _context.User.Identity?.Name ?? SchemaDefaults.SystemUserName));
                if (null != newId)
                {
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, (Guid)newId));
                }
                if (null != typedef)
                {
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainTypeDefDefaults.ParamTypeDefId, typedef?.Id));
                }

                using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (await rs.ReadAsync(cancellationToken))
                    {
                        result = new GrainBase(new GrainExtendedDataAdapter(rs, GrainExtendedDataAdapter.ExtensionColumn.None));
                    }
                }
            }
            return result;
        }

        protected async Task<int> StoreGrainsInTA(IEnumerable<IGrainBase> grainsMod, int result, DbTransaction ta, bool aclWasChecked = false, CancellationToken cancellationToken = default)
        {
            if (grainsMod.Any())
            {
                if (!aclWasChecked && !await _accessService.VerfifyAccessAsync(grainsMod, GrainAccessFlag.Write, cancellationToken))
                {
                    throw new SchemaAccessDeniedException(GrainAccessFlag.Write);
                }
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    var idCol = MapGrainBaseColumn(nameof(IGrainBase.Id));
                    foreach (var grain in grainsMod)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = GrainBaseConfig.SQLUpdate;
                        cmd.CommandText += _profile.ParameterFactory.PrepareDirtyFieldsUpdate<GrainExtendedDataAdapter, IGrainBase>(cmd.Parameters, grain);
                        cmd.CommandText += $" WHERE {idCol} = @{GeneralEntityDefaults.ParamId}";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grain.Id));

                        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                        result += affected;
                        if (0 < affected)
                        {
                            grain.GetDirtyFields<IGrainBase>().Clear();
                        }
                    }
                }
            }

            return result;
        }

        protected async Task<int> DeleteGrainsInTA(IEnumerable<IIdentifiable> grains, int result, DbTransaction ta, bool aclWasChecked = false, CancellationToken cancellationToken = default)
        {
            if (grains.Any())
            {
                CheckBuiltIns(grains);
                if (!aclWasChecked && !await _accessService.VerfifyAccessAsync(grains, GrainAccessFlag.Delete, cancellationToken))
                {
                    throw new SchemaAccessDeniedException(GrainAccessFlag.Delete);
                }
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    cmd.CommandText = $"{GrainBaseConfig.SQLDelete}{MapGrainBaseColumn(nameof(IGrainBase.Id))} = @{GeneralEntityDefaults.ParamId}";
                    var param = _profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, (Guid?)null);
                    cmd.Parameters.Add(param);

                    foreach (var grain in grains)
                    {
                        _profile.ParameterFactory.Update(param, grain.Id);
                        result += await cmd.ExecuteNonQueryAsync(cancellationToken);
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("DeleteGrainsAsync id={id}, success={success}", cmd.Parameters[0].Value, result);
                        }
                    }
                }
            }
            return result;
        }

        protected async Task<int> StoreLocalizedGrainsInTA(IEnumerable<IGrainLocalized> grainsModL, int result, DbTransaction ta, CancellationToken cancellationToken)
        {
            foreach (var lgrain in grainsModL)
            {
                var affected = await StoreGrainLabelInTA(ta, lgrain.Id, lgrain.Culture ?? SchemaDefaults.Culture.Name, lgrain.Label, cancellationToken);
                result += affected;
                if (0 < affected)
                {
                    lgrain.GetDirtyFields<IGrainLocalized>().Clear();
                }
            }

            return result;
        }

        protected async Task<int> StoreGrainLabelInTA(DbTransaction ta, Guid grainId, string lang, string? label, CancellationToken cancellationToken)
        {
            using (var cmd = ta.Connection!.CreateCommand())
            {
                if (string.IsNullOrEmpty(label))
                {
                    cmd.CommandText = $"{GrainLocalizedConfig<TDialect>.SQLDeleteLabel}{GeneralEntityDefaults.FieldGrainId} = @{GeneralEntityDefaults.ParamId} AND {AbstractDataAdapter.GetAdapterColumnName<GrainLocalizedDataAdapter>(nameof(IGrainLocalized.CultureInfo))} = @{GeneralEntityDefaults.ParamLangCode}";
                }
                else
                {
                    cmd.CommandText = GrainLocalizedConfig<TDialect>.SQLUpdateLabel;
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainLocalizedDefaults.ParamLabel, label));
                }
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grainId));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamLangCode, lang));

                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        protected async Task<int> DeleteGrainLabelsInTA(DbTransaction ta, Guid grainId, CancellationToken cancellationToken)
        {
            using (var cmd = ta.Connection!.CreateCommand())
            {
                cmd.CommandText = $"{GrainLocalizedConfig<TDialect>.SQLDeleteLabel}{GeneralEntityDefaults.FieldGrainId} = @{GeneralEntityDefaults.ParamId}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grainId));
                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        protected async Task<int> DisableGrainTimestampTriggers(DbTransaction ta, Guid grainId, CancellationToken cancellationToken)
        {
            using (var cmd = ta.Connection!.CreateCommand())
            {
                const string flagCol = "flag";

                cmd.CommandText = @$"INSERT INTO mb_grain_control ({GeneralEntityDefaults.FieldGrainId}, {flagCol}) VALUES (@{GeneralEntityDefaults.ParamGrainId}, @{flagCol})
ON CONFLICT ({GeneralEntityDefaults.FieldGrainId}) DO UPDATE SET {flagCol} = {EngineSpec<TDialect>.Dialect.ConflictExcluded(flagCol)}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, grainId));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(flagCol, 0x1));
                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        protected async Task<int> EnableGrainTimestampTriggers(DbTransaction ta, Guid grainId, CancellationToken cancellationToken)
        {
            using (var cmd = ta.Connection!.CreateCommand())
            {
                cmd.CommandText = $"DELETE FROM mb_grain_control WHERE {GeneralEntityDefaults.FieldGrainId} = @{GeneralEntityDefaults.ParamGrainId}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, grainId));
                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public static async IAsyncEnumerable<TIGrain> EnumGrainDataReader<TIGrain, TGrain, TGrainAdapter>(DbDataReader reader, GrainExtendedDataAdapter.ExtensionColumn extensionColumn = GrainExtendedDataAdapter.ExtensionColumn.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
            where TIGrain : IGrainBase
            where TGrain : GrainBase
            where TGrainAdapter : GrainExtendedDataAdapter
        {
            while (!reader.IsClosed && await reader.ReadAsync(cancellationToken))
            {
                TGrainAdapter dataGrain = GrainExtendedDataAdapter.ExtensionColumn.All == extensionColumn
                    ? (TGrainAdapter)Activator.CreateInstance(typeof(TGrainAdapter), reader)!
                    : (TGrainAdapter)Activator.CreateInstance(typeof(TGrainAdapter), reader, extensionColumn)!;
                yield return (TIGrain)Activator.CreateInstance(typeof(TGrain), dataGrain)!;
            }
        }

        public string PrepareGrainQueryFilterParameters(DbParameterCollection parameters, IGrainQueryFilter? filter = null, string? fieldPrefix = null)
        {
            var sb = new StringBuilder();
            if (null != filter?.TypeConstraints && filter.TypeConstraints.Any())
            {
                sb.Append(" AND (");
                var sep = string.Empty;
                var i = 0;
                var nameCol = MapGrainBaseColumn(nameof(IGrainBase.TypeName));
                var idCol = MapGrainBaseColumn(nameof(IGrainBase.TypeDefId));
                foreach (var constraint in filter.TypeConstraints)
                {
                    if (!string.IsNullOrEmpty(constraint.TypeName))
                    {
                        if (SchemaDefaults.TypeDefTypeName.Equals(constraint.TypeName, StringComparison.Ordinal))
                        {
                            sb.Append($"{sep}{nameCol} IS NULL");
                        }
                        else
                        {
                            var param = _profile.ParameterFactory.Create($"typeName{i}", constraint.TypeName);
                            parameters.Add(param);
                            sb.Append($"{sep}{nameCol} = @{param.ParameterName}");
                        }
                    }
                    else if (null == constraint.TypeDefId)
                    {
                        sb.Append($"{sep}{idCol} IS NULL");
                    }
                    else
                    {
                        var param = _profile.ParameterFactory.Create($"{GrainTypeDefDefaults.ParamTypeDefId}{i}", constraint.TypeDefId);
                        parameters.Add(param);
                        sb.Append($"{sep}{idCol} = @{param.ParameterName} OR {idCol} IN ({GrainTypeDefConfig<TDialect>.SQLSelectTypeDefMixinDescendants} m.{GrainTypeDefDefaults.MixinExtFieldStart} = @{param.ParameterName})");
                    }
                    if (0 == sep.Length)
                    {
                        sep = " OR ";
                    }
                    i++;
                }
                sb.Append(')');
            }
            if (null != filter?.IdConstraints && filter.IdConstraints.Any())
            {
                sb.Append($" AND {(string.IsNullOrEmpty(fieldPrefix) ? string.Empty : $"{fieldPrefix}.")}{GeneralEntityDefaults.FieldId} IN (");
                var sep = string.Empty;
                var paramPfx = "idFilter";
                var i = 0;
                foreach (var id in filter.IdConstraints)
                {
                    var param = _profile.ParameterFactory.Create($"{paramPfx}{i++}", id);
                    parameters.Add(param);
                    sb.Append($"{sep}@{param.ParameterName}");
                    if (0 == sep.Length)
                    {
                        sep = ", ";
                    }
                }
                sb.Append(')');
            }
            if (null != filter?.MTimeConstraint?.Start || null != filter?.MTimeConstraint?.End)
            {
                var mTimeCol = MapGrainBaseColumn(nameof(IGrainBase.MTime));
                if (!string.IsNullOrEmpty(fieldPrefix))
                {
                    mTimeCol = $"{fieldPrefix}.{mTimeCol}";
                }
                void tsFunc(string comp, bool include, DateTime ts)
                {
                    var param = _profile.ParameterFactory.Create($"mtime{(">" == comp ? "Start" : "End")}", ts);
                    parameters.Add(param);

                    sb.Append($" AND {EngineSpec<TDialect>.Dialect.ComparableDate(mTimeCol)} {comp}");
                    if (include)
                    {
                        sb.Append('=');
                    }
                    sb.Append($" {EngineSpec<TDialect>.Dialect.ComparableDate($"@{param.ParameterName}")}");
                }
                if (null != filter.MTimeConstraint.Start)
                {
                    tsFunc(">", RangeInclusionFlag.Start == (filter.MTimeConstraint.Including & RangeInclusionFlag.Start), (DateTime)filter.MTimeConstraint.Start);
                }
                if (null != filter.MTimeConstraint.End)
                {
                    tsFunc("<", RangeInclusionFlag.End == (filter.MTimeConstraint.Including & RangeInclusionFlag.End), (DateTime)filter.MTimeConstraint.End);
                }
            }
            return sb.ToString();
        }

        protected static string MapGrainBaseColumn(string fieldName)
        {
            return AbstractDataAdapter.GetAdapterColumnName<GrainExtendedDataAdapter>(fieldName);
        }
        #endregion
    }
}
