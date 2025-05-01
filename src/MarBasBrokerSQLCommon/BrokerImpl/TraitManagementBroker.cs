using CraftedSolutions.MarBasBrokerSQLCommon.Grain;
using CraftedSolutions.MarBasBrokerSQLCommon.GrainDef;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Broker;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.Grain.Traits;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Globalization;

namespace CraftedSolutions.MarBasBrokerSQLCommon.BrokerImpl
{
    public abstract class TraitManagementBroker<TDialect>
        : GrainManagementBroker<TDialect>, ITraitManagementBroker, IAsyncTraitManagementBroker
        where TDialect : ISQLDialect, new()
    {
        protected TraitManagementBroker(IBrokerProfile profile, ILogger logger) : base(profile, logger)
        {
        }

        protected TraitManagementBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger logger) : base(profile, context, accessService, logger)
        {
        }

        public GrainTraitsMap GetGrainTraits(IIdentifiable grain, CultureInfo? culture = null)
        {
            return GetGrainTraitsAsync(grain, culture).Result;
        }

        public async Task<GrainTraitsMap> GetGrainTraitsAsync(IIdentifiable grain, CultureInfo? culture = null, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            if (!await _accessService.VerfifyAccessAsync(new[] { grain }, GrainAccessFlag.Read, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.Read);
            }
            GrainTraitsMap result = new();
            return await ExecuteOnConnection(result, async (cmd) =>
            {
                using (cmd)
                {
                    cmd.CommandText = TraitBaseConfig<TDialect>.SQLSelectWithDefaultsByGrain;
                    _profile.ParameterFactory.AddParametersForCultureLayer(cmd.Parameters, culture);
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, grain.Id));
                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await rs.ReadAsync(cancellationToken))
                        {
                            result.Set(ReadTrait(rs), rs.GetString(rs.GetOrdinal(MapGrainBaseColumn(nameof(IGrainBase.Name)))));
                        }
                    }
                }
                return result;

            }, cancellationToken);
        }

        public ITraitBase? GetTrait(Guid id)
        {
            return GetTraitAsync(id).Result;
        }

        public async Task<ITraitBase?> GetTraitAsync(Guid id, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            return await ExecuteOnConnection(null, async (cmd) =>
            {
                using (cmd)
                {

                    cmd.CommandText = $"{TraitBaseConfig<TDialect>.SQLSelect}{GeneralEntityDefaults.FieldId}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, id));
                    if (await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.SkipPermissionCheck, cancellationToken: cancellationToken))
                    {
                        cmd.CommandText += $" = @{GeneralEntityDefaults.ParamId}";
                    }
                    else
                    {
                        cmd.CommandText += $" IN ({TraitBaseConfig<TDialect>.SQLFilterByAcl}_t.{GeneralEntityDefaults.FieldId} = @{GeneralEntityDefaults.ParamId})";
                        _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, (await _accessService.GetContextPrimaryRoleAsync(cancellationToken)).Id);
                    }

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            return ReadTrait(rs);
                        }
                    }
                }
                return null;
            }, cancellationToken);
        }

        public int DeleteTraits(IEnumerable<IIdentifiable> ids)
        {
            return DeleteTraitsAsync(ids).Result;
        }

        public async Task<int> DeleteTraitsAsync(IEnumerable<IIdentifiable> ids, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            CheckBuiltIns(ids);
            var result = 0;
            result = await WrapInTransaction(result, async (ta) =>
            {
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    cmd.CommandText = $"{TraitBaseConfig<TDialect>.SQLDelete}{GeneralEntityDefaults.FieldId}";
                    var param = _profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, (Guid?)null);
                    cmd.Parameters.Add(param);
                    if (await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.SkipPermissionCheck, cancellationToken: cancellationToken))
                    {
                        cmd.CommandText += $" = @{GeneralEntityDefaults.ParamId}";
                    }
                    else
                    {
                        cmd.CommandText += $" IN ({TraitBaseConfig<TDialect>.SQLFilterByAcl}_t.{GeneralEntityDefaults.FieldId} = @{GeneralEntityDefaults.ParamId})";
                        _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, (await _accessService.GetContextPrimaryRoleAsync(cancellationToken)).Id, GrainAccessFlag.Write | GrainAccessFlag.WriteTraits);
                    }

                    foreach (var id in ids)
                    {
                        _profile.ParameterFactory.Update(param, id);
                        result += await cmd.ExecuteNonQueryAsync(cancellationToken);
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("DeleteTraitsAsync id={id}, success={success}", cmd.Parameters[0].Value, result);
                        }
                    }
                }
                return result;
            }, cancellationToken);
            return result;
        }

        public int StoreTraits(IEnumerable<ITraitBase> traits)
        {
            return StoreTraitsAsync(traits).Result;
        }

        public async Task<int> StoreTraitsAsync(IEnumerable<ITraitBase> traits, CancellationToken cancellationToken = default)
        {
            var traitsMod = traits.Where(t => 0 < t.GetDirtyFields<ITraitBase>().Count);
            if (!traitsMod.Any())
            {
                return -1;
            }
            CheckProfile();
            if (!await _accessService.VerfifyAccessAsync(traits.Select(x => x.Grain), GrainAccessFlag.Write, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.Write);
            }
            if (!await _accessService.VerfifyAccessAsync(traits.Select(x => x.PropDef), GrainAccessFlag.WriteTraits, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.WriteTraits);
            }
            var result = 0;
            var colMapper = new TraitBaseDataAdapter.ColumnMapper();
            result = await WrapInTransaction(result, async (ta) =>
            {
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    foreach (var trait in traitsMod)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = TraitBaseConfig<TDialect>.SQLUpdate;
                        cmd.CommandText += _profile.ParameterFactory.PrepareDirtyFieldsUpdate<TraitBaseDataAdapter, ITraitBase>(cmd.Parameters, trait, colMapper);
                        cmd.CommandText += $" WHERE {MapTraitColumn(nameof(IIdentifiable.Id))} = @{GeneralEntityDefaults.ParamId}";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, trait.Id));

                        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                        result += affected;
                        if (0 < affected)
                        {
                            trait.GetDirtyFields<IGrainBase>().Clear();
                        }
                    }
                }
                return result;
            }, cancellationToken);
            return result;
        }

        public ITraitBase? CreateTrait(ITraitRef traitRef, object? value = null, int ord = 0)
        {
            return CreateTraitAsync(traitRef, value, ord).Result;
        }

        public async Task<ITraitBase?> CreateTraitAsync(ITraitRef traitRef, object? value = null, int ord = 0, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            return await WrapInTransaction(null, async (ta) =>
            {
                var result = await CreateTraitInTA(ta, traitRef, value, ord, cancellationToken: cancellationToken);
                await ReindexTraitsInTA(ta, (Identifiable)traitRef.GrainId, (Identifiable)traitRef.PropDef, traitRef.CultureInfo, traitRef.Revision, false, cancellationToken);
                return result;
            }, cancellationToken);
        }

        public IEnumerable<ITraitBase> GetTraitValues(ITraitRef traitRef)
        {
            return GetTraitValuesAsync(traitRef).Result;
        }

        public async Task<IEnumerable<ITraitBase>> GetTraitValuesAsync(ITraitRef traitRef, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            if (!await _accessService.VerfifyAccessAsync(new[] { traitRef.Grain }, GrainAccessFlag.Read, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.Read);
            }
            var result = new List<ITraitBase>();
            return await ExecuteOnConnection(result, async (cmd) =>
            {
                using (cmd)
                {
                    cmd.CommandText = TraitBaseConfig<TDialect>.SQLSelectWithDefaultsByPropDef;
                    if (0 < traitRef.Revision)
                    {
                        var revCol = MapGrainBaseColumn(nameof(ITraitRef.Revision));
                        cmd.CommandText += $"AND ({revCol} = @{GeneralEntityDefaults.ParamRevision} OR {revCol} < 1) ";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamRevision, traitRef.Revision));
                    }
                    cmd.CommandText += TraitBaseConfig<TDialect>.SQLOrderByGrain;

                    _profile.ParameterFactory.AddParametersForCultureLayer(cmd.Parameters, traitRef.CultureInfo);
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, traitRef.GrainId));
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(TraitBaseDefaults.ParamPropDefId, traitRef.PropDefId));
                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await rs.ReadAsync(cancellationToken))
                        {
                            result.Add(ReadTrait(rs));
                        }
                    }
                }
                return result;
            }, cancellationToken);
        }

        public int ReplaceTraitValues<T>(ITraitRef traitRef, IEnumerable<T> values)
        {
            return ReplaceTraitValuesAsync(traitRef, values).Result;
        }

        public async Task<int> ReplaceTraitValuesAsync<T>(ITraitRef traitRef, IEnumerable<T> values, CancellationToken cancellationToken = default)
        {
            if (!values.Any())
            {
                return -1;
            }
            CheckProfile();
            if (!await _accessService.VerfifyAccessAsync(new[] { traitRef.Grain }, GrainAccessFlag.Write, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.Write);
            }
            if (!await _accessService.VerfifyAccessAsync(new[] { traitRef.PropDef }, GrainAccessFlag.WriteTraits, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.WriteTraits);
            }

            var result = 0;

            result = await WrapInTransaction(result, async (ta) =>
            {
                result = await DeleteTraitsByRefInTA(ta, traitRef, true, cancellationToken);

                using (var cmd = ta.Connection!.CreateCommand())
                {
                    var valueType = await GetPropDefValueType(ta.Connection!, traitRef.PropDef, cancellationToken);

                    var cols = new string[]
                    {
                        MapTraitColumn(nameof(ITraitBase.Id)),
                        MapTraitColumn(nameof(ITraitBase.GrainId)),
                        MapTraitColumn(nameof(ITraitBase.PropDefId)),
                        MapTraitColumn(nameof(ITraitBase.Culture)),
                        MapTraitColumn(nameof(ITraitBase.Revision)),
                        MapTraitColumn(nameof(ITraitBase.Ord)),
                       TraitBaseDataAdapter.GetValueColumn(valueType)
                    };
                    var vals = new string[]
                    {
                        GeneralEntityDefaults.ParamGrainId,
                        TraitBaseDefaults.ParamPropDefId,
                        GeneralEntityDefaults.ParamLangCode,
                        GeneralEntityDefaults.ParamRevision
                    };

                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, traitRef.GrainId));
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(TraitBaseDefaults.ParamPropDefId, traitRef.PropDefId));
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamLangCode, traitRef.Culture));
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamRevision, traitRef.Revision));

                    var i = 0;
                    var valsClause = values.Aggregate(string.Empty, (aggr, curr) =>
                    {
                        if (0 < aggr.Length)
                        {
                            aggr += ", ";
                        }
                        aggr += $"({EngineSpec<TDialect>.Dialect.GuidGen}, @{string.Join(", @", vals)}";
                        var paramName = $"{TraitBaseDefaults.ParamOrd}{i}";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(paramName, i));

                        aggr += $", @{paramName}";

                        var val = TraitValueFactory.ConvertValue(valueType, curr);
                        paramName = $"{TraitBaseDefaults.ParamValue}{i}";
                        cmd.Parameters.Add(_profile.ParameterFactory.PrepareTraitValueParameter(paramName, valueType, val));

                        aggr += $", @{paramName})";

                        i++;
                        return aggr;
                    });

                    cmd.CommandText = $"{TraitBaseConfig<TDialect>.SQLInsert}({string.Join(", ", cols)}) VALUES {valsClause}";
                    result += await cmd.ExecuteNonQueryAsync(cancellationToken);
                }
                result -= await ReindexTraitsInTA(ta, traitRef.Grain, traitRef.PropDef, traitRef.CultureInfo ?? CultureInfo.InvariantCulture, traitRef.Revision, true, cancellationToken);
                return result;
            }, cancellationToken);
            return result;
        }

        public int ResetTraitValues(ITraitRef traitRef)
        {
            return ResetTraitValuesAsync(traitRef).Result;
        }

        public async Task<int> ResetTraitValuesAsync(ITraitRef traitRef, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            return await WrapInTransaction(0, async (ta) => await DeleteTraitsByRefInTA(ta, traitRef, cancellationToken: cancellationToken), cancellationToken);
        }

        public int ReindexTraits(IIdentifiable grain, IIdentifiable? propDef = null, CultureInfo? culture = null, int revision = -1, bool trimOverflow = false)
        {
            return ReindexTraitsAsync(grain, propDef, culture, revision, trimOverflow).Result;
        }

        public async Task<int> ReindexTraitsAsync(IIdentifiable grain, IIdentifiable? propDef = null, CultureInfo? culture = null, int revision = -1, bool trimOverflow = false, CancellationToken cancellationToken = default)
        {
            return await WrapInTransaction(0, async (ta) =>
            {
                return await ReindexTraitsInTA(ta, grain, propDef, culture, revision, trimOverflow, cancellationToken);
            }, cancellationToken);
        }

        public IEnumerable<IGrainLocalized> LookupGrainsByTrait(ITraitRef traitRef, object? value = null, IEnumerable<IListSortOption<GrainSortField>>? sortOptions = null)
        {
            return LookupGrainsByTraitAsync(traitRef, value, sortOptions).Result;
        }

        public async Task<IEnumerable<IGrainLocalized>> LookupGrainsByTraitAsync(ITraitRef traitRef, object? value = null, IEnumerable<IListSortOption<GrainSortField>>? sortOptions = null, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            return await ExecuteOnConnection<IEnumerable<IGrainLocalized>>(Enumerable.Empty<IGrainLocalized>(), async (cmd) =>
            {
                using (cmd)
                {
                    var valType = (traitRef.PropDef as IValueTypeConstraint)?.ValueType ?? (traitRef as IValueTypeConstraint)?.ValueType ?? TraitValueType.Text;

                    cmd.CommandText = @$"{GrainLocalizedConfig<TDialect>.SQLSelectByAclLocalizedTrunk}
JOIN ({TraitBaseConfig<TDialect>.SQLSelectMeta}) AS t
ON t.{GeneralEntityDefaults.FieldGrainId} = g.{GeneralEntityDefaults.FieldId} AND t.{GeneralEntityDefaults.FieldRevision} = g.{GeneralEntityDefaults.FieldRevision}
WHERE t.{GeneralEntityDefaults.FieldRevision} = @{GeneralEntityDefaults.ParamRevision} AND t.{TraitBaseDataAdapter.GetValueColumn(valType)}";
                    cmd.CommandText += null == value ? " IS NULL" : $" = @{TraitBaseDefaults.ParamValue}";

                    var orderBy = PrepareListOrderByClause<GrainSortField, GrainLocalizedDataAdapter>(sortOptions, "g");
                    if (string.IsNullOrEmpty(orderBy))
                    {
                        orderBy = $" ORDER BY g.{MapGrainBaseColumn(nameof(IGrainBase.Path))}";
                    }
                    cmd.CommandText += orderBy;

                    _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, (await _accessService.GetContextPrimaryRoleAsync(cancellationToken)).Id);
                    _profile.ParameterFactory.AddParametersForCultureLayer(cmd.Parameters, traitRef.CultureInfo);

                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamRevision, traitRef.Revision));
                    if (null != value)
                    {
                        cmd.Parameters.Add(_profile.ParameterFactory.PrepareTraitValueParameter(TraitBaseDefaults.ParamValue, valType, value));
                    }

                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        _logger.LogTrace("LookupGrainsByTrait: {sql}", cmd.CommandText);
                    }

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        return await EnumGrainDataReader<IGrainLocalized, GrainLocalized, GrainLocalizedDataAdapter>(rs, GrainExtendedDataAdapter.ExtensionColumn.All, cancellationToken).ToListAsync(cancellationToken);
                    }
                }
            }, cancellationToken);
        }

    #region Helper Methods

    protected async Task<ITraitBase?> CreateTraitInTA(DbTransaction ta, ITraitRef traitRef, object? value = null, int ord = 0, bool aclWasChecked = false, Guid? useId = null, CancellationToken cancellationToken = default)
        {
            if (!aclWasChecked)
            {
                if (!await _accessService.VerfifyAccessAsync(new[] { traitRef.Grain }, GrainAccessFlag.Write, cancellationToken))
                {
                    throw new SchemaAccessDeniedException(GrainAccessFlag.Write);
                }
                if (!await _accessService.VerfifyAccessAsync(new[] { traitRef.PropDef }, GrainAccessFlag.WriteTraits, cancellationToken))
                {
                    throw new SchemaAccessDeniedException(GrainAccessFlag.WriteTraits);
                }
            }
            var valType = (traitRef.PropDef as IValueTypeConstraint)?.ValueType ?? (traitRef as IValueTypeConstraint)?.ValueType ?? TraitValueType.Text;
            ITraitBase? result = null;
            using (var cmd = ta.Connection!.CreateCommand())
            {
                var cols = new List<string>()
                    {
                        MapTraitColumn(nameof(ITraitBase.Id)),
                        MapTraitColumn(nameof(ITraitBase.GrainId)),
                        MapTraitColumn(nameof(ITraitBase.PropDefId))
                    };
                var vals = new List<string>() { GeneralEntityDefaults.ParamGrainId, TraitBaseDefaults.ParamPropDefId };

                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, traitRef.GrainId));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(TraitBaseDefaults.ParamPropDefId, traitRef.PropDefId));
                if (null != useId)
                {
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, useId));
                }


                if (null != traitRef.CultureInfo)
                {
                    cols.Add(MapTraitColumn(nameof(ITraitBase.CultureInfo)));
                    vals.Add(GeneralEntityDefaults.ParamLangCode);
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(vals.Last(), traitRef.Culture));
                }
                if (0 != ord)
                {
                    cols.Add(MapTraitColumn(nameof(ITraitBase.Ord)));
                    vals.Add(TraitBaseDefaults.ParamOrd);
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(vals.Last(), ord));
                }
                if (1 != traitRef.Revision)
                {
                    cols.Add(MapTraitColumn(nameof(ITraitBase.Revision)));
                    vals.Add(GeneralEntityDefaults.ParamRevision);
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(vals.Last(), traitRef.Revision));
                }
                if (null != value)
                {
                    cols.Add(TraitBaseDataAdapter.GetValueColumn(valType));
                    vals.Add(TraitBaseDefaults.ParamValue);
                    cmd.Parameters.Add(_profile.ParameterFactory.PrepareTraitValueParameter(vals.Last(), valType, value));
                }

                cmd.CommandText = $"{TraitBaseConfig<TDialect>.SQLInsert}({string.Join(",", cols)}) VALUES ({(null == useId ? EngineSpec<TDialect>.Dialect.GuidGen : $"@{GeneralEntityDefaults.ParamId}")}, @{string.Join(",@", vals)}){EngineSpec<TDialect>.Dialect.ReturnFromInsert}";

                using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (await rs.ReadAsync(cancellationToken))
                    {
                        result = ReadTrait(rs, Enum.GetName(valType)?.ToLowerInvariant());
                    }
                }
            }
            return result;
        }

        protected async Task<int> DeleteTraitsByRefInTA(DbTransaction ta, ITraitRef traitRef, bool aclWasChecked = false, CancellationToken cancellationToken = default)
        {
            if (!aclWasChecked)
            {
                if (!await _accessService.VerfifyAccessAsync(new[] { traitRef.Grain }, GrainAccessFlag.Write, cancellationToken))
                {
                    throw new SchemaAccessDeniedException(GrainAccessFlag.Write);
                }
                if (!await _accessService.VerfifyAccessAsync(new[] { traitRef.PropDef }, GrainAccessFlag.WriteTraits, cancellationToken))
                {
                    throw new SchemaAccessDeniedException(GrainAccessFlag.WriteTraits);
                }
            }
            using (var cmd = ta.Connection!.CreateCommand())
            {
                cmd.CommandText = $"{TraitBaseConfig<TDialect>.SQLDelete}{MapTraitColumn(nameof(ITraitBase.GrainId))} = @{GeneralEntityDefaults.ParamGrainId} AND {MapTraitColumn(nameof(ITraitRef.PropDefId))} = @{TraitBaseDefaults.ParamPropDefId} AND {MapTraitColumn(nameof(ITraitRef.Revision))} = @{GeneralEntityDefaults.ParamRevision}";
                if (null == traitRef.Culture)
                {
                    cmd.CommandText += $" AND {MapTraitColumn(nameof(ITraitRef.Culture))} IS NULL";
                }
                else
                {
                    cmd.CommandText += $" AND {MapTraitColumn(nameof(ITraitRef.Culture))} = @{GeneralEntityDefaults.ParamLangCode}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamLangCode, traitRef.Culture));
                }
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, traitRef.GrainId));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(TraitBaseDefaults.ParamPropDefId, traitRef.PropDefId));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamRevision, traitRef.Revision));

                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        protected async Task<int> DeleteGrainTraitsInTA(DbTransaction ta, Guid grainId, bool aclWasChecked = false, CancellationToken cancellationToken = default)
        {
            if (!aclWasChecked && !await _accessService.VerfifyAccessAsync(new[] { (Identifiable)grainId }, GrainAccessFlag.Write, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.Write);
            }
            using (var cmd = ta.Connection!.CreateCommand())
            {
                cmd.CommandText = $"{TraitBaseConfig<TDialect>.SQLDelete}{MapTraitColumn(nameof(ITraitBase.GrainId))} = @{GeneralEntityDefaults.ParamGrainId}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, grainId));
                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        protected async Task<int> ReindexTraitsInTA(DbTransaction ta, IIdentifiable grain, IIdentifiable? propDef = null, CultureInfo? culture = null, int revision = -1, bool trimOverflow = false, CancellationToken cancellationToken = default)
        {
            var result = 0;
            using (var cmd = ta.Connection!.CreateCommand())
            {
                var propIdCol = MapTraitColumn(nameof(ITrait.PropDefId));
                var whereAdditions = string.Empty;

                cmd.CommandText = $"{TraitBaseConfig<TDialect>.SQLUpdateTraitReindex} WHERE {GeneralEntityDefaults.FieldGrainId} = @{GeneralEntityDefaults.ParamGrainId}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, grain.Id));
                if (null != propDef)
                {
                    whereAdditions += $" AND {propIdCol} = @{TraitBaseDefaults.ParamPropDefId}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(TraitBaseDefaults.ParamPropDefId, propDef.Id));
                }
                if (null != culture)
                {
                    whereAdditions += $" AND {GeneralEntityDefaults.FieldLangCode}";
                    if (CultureInfo.InvariantCulture == culture)
                    {
                        whereAdditions += " IS NULL";
                    }
                    else
                    {
                        whereAdditions += $" = @{GeneralEntityDefaults.ParamLangCode}";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamLangCode, culture.IetfLanguageTag));
                    }
                }
                if (-1 < revision)
                {
                    whereAdditions += $" AND {GeneralEntityDefaults.FieldRevision} = @{GeneralEntityDefaults.ParamRevision}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamRevision, revision));
                }
                cmd.CommandText += whereAdditions;

                _ = await cmd.ExecuteNonQueryAsync(cancellationToken);

                if (trimOverflow)
                {
                    cmd.CommandText = @$"{TraitBaseConfig<TDialect>.SQLDelete}{GeneralEntityDefaults.FieldGrainId} = @{GeneralEntityDefaults.ParamGrainId}{whereAdditions}
AND {MapTraitColumn(nameof(ITrait.Ord))} >= (SELECT {EngineSpec<TDialect>.Dialect.SignedToUnsigned($"p.{GrainPropDefDefaults.FieldCardinalityMax}")} FROM {GrainPropDefDefaults.DataSourcePropDef} p WHERE p.{GeneralEntityDefaults.FieldBaseId} = t.{propIdCol})";

                    result = await cmd.ExecuteNonQueryAsync(cancellationToken);
                    if (0 < result && _logger.IsEnabled(LogLevel.Warning))
                    {
                        _logger.LogWarning("{result} trait value(s) overflowing max. cardinality have been deleted from grain {grainId} (property {property})",
                            result, grain.Id, propDef?.Id.ToString("D") ?? "ALL");
                    }
                }
            }
            return result;
        }

        protected async Task<TraitValueType> GetPropDefValueType(DbConnection conn, IIdentifiable propDef, CancellationToken cancellationToken)
        {
            if (propDef is IValueTypeConstraint constraint)
            {
                return constraint.ValueType;
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"{GrainPropDefConfig<TDialect>.SQLSelectPropDef}{GeneralEntityDefaults.FieldId} = @{GeneralEntityDefaults.ParamId}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, propDef.Id));
                using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (await rs.ReadAsync(cancellationToken))
                    {
                        return TraitValueFactory.GetValueTypeFromString(rs.GetString(rs.GetOrdinal(MapTraitColumn(nameof(ITraitBase.ValueType)))));
                    }
                }
            }
            return TraitValueType.Text;
        }

        protected static string MapTraitColumn(string fieldName)
        {
            return AbstractDataAdapter.GetAdapterColumnName<TraitBaseDataAdapter>(fieldName);
        }

        protected static ITraitBase ReadTrait(DbDataReader reader, string? valueType = null)
        {
            var adapter = TraitBaseDataAdapter.Create(reader, valueType);
            var result = adapter.Adapt();
            if (null == result)
            {
                throw new NotSupportedException($"Unknown trait type {adapter.ValueType}");
            }
            return result;
        }
        #endregion
    }
}
