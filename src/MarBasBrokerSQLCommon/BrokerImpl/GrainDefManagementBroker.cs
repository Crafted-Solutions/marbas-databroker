using System.Data.Common;
using System.Globalization;
using MarBasBrokerSQLCommon.Grain;
using MarBasBrokerSQLCommon.GrainDef;
using MarBasCommon;
using MarBasSchema;
using MarBasSchema.Access;
using MarBasSchema.Broker;
using MarBasSchema.Grain;
using MarBasSchema.Grain.Traits;
using MarBasSchema.GrainDef;
using Microsoft.Extensions.Logging;

namespace MarBasBrokerSQLCommon.BrokerImpl
{
    public abstract class GrainDefManagementBroker<TDialect>
        : TraitManagementBroker<TDialect>, IGrainDefManagementBroker, IAsyncGrainDefManagementBroker
        where TDialect : ISQLDialect, new()
    {
        protected GrainDefManagementBroker(IBrokerProfile profile, ILogger logger) : base(profile, logger)
        {
        }

        protected GrainDefManagementBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger logger) : base(profile, context, accessService, logger)
        {
        }

        #region TypeDef Management
        public IGrainTypeDefLocalized? GetTypeDef(Guid id, CultureInfo? culture = null)
        {
            return GetTypeDefAsync(id, culture).Result;
        }

        public async Task<IGrainTypeDefLocalized?> GetTypeDefAsync(Guid id, CultureInfo? culture = null, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            using (var conn = _profile.Connection)
            {
                await conn.OpenAsync(cancellationToken);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"{GrainTypeDefConfig<TDialect>.SQLSelectTypeDefByAclLocalized}g.{MapGrainBaseColumn(nameof(IGrainBase.Id))} = @{GeneralEntityDefaults.ParamId}";
                    _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, (await _accessService.GetContextPrimaryRoleAsync(cancellationToken)).Id);
                    _profile.ParameterFactory.AddParametersForCultureLayer(cmd.Parameters, culture);
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, id));
                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("GetTypeDefAsync id={id}, HasRows={hasRows}", id.ToString("D"), rs.HasRows);
                        }
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            var typedef = new GrainTypeDefDataAdapter(rs);
                            await GetTypeDefMixins(typedef, cancellationToken);
                            return new GrainTypeDef(typedef);
                        }
                    }
                }
            }
            return null;
        }

        public IGrainTypeDef? CreateTypeDef(string name, IIdentifiable? parent, string? implKey = null, IEnumerable<IIdentifiable>? mixins = null)
        {
            return CreateTypeDefAsync(name, parent, implKey, mixins).Result;
        }

        public async Task<IGrainTypeDef?> CreateTypeDefAsync(string name, IIdentifiable? parent, string? implKey = null, IEnumerable<IIdentifiable>? mixins = null, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            IGrainTypeDef? result = null;
            result = await WrapInTransaction(result, async (ta) =>
            {
                var grain = await CreateGrainInTA(name, parent ?? (Identifiable)SchemaDefaults.UserSchemaContainerID, null, ta, cancellationToken: cancellationToken);
                if (null != grain)
                {
                    result = new GrainTypeDef(grain)
                    {
                        Impl = implKey
                    };
                    if (null != mixins)
                    {
                        _ = mixins.TakeWhile((mixin) => { result.AddMixIn(mixin); return true; });
                    }
                    result.GetDirtyFields<IGrainTypeDef>().Clear();
                    using (var cmd = ta.Connection!.CreateCommand())
                    {
                        var cols = new List<string>() { GeneralEntityDefaults.FieldBaseId };
                        var vals = new List<string>() { GeneralEntityDefaults.ParamId };
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grain.Id));
                        if (!string.IsNullOrEmpty(implKey))
                        {
                            cols.Add(MapTypeDefColumn(nameof(IGrainTypeDef.Impl)));
                            vals.Add(GrainTypeDefDefaults.ParamImpl);
                            cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainTypeDefDefaults.ParamImpl, implKey));
                        }
                        cmd.CommandText = $"{GrainTypeDefConfig<TDialect>.SQLInsertTypeDef} ({string.Join(",", cols)}) VALUES (@{string.Join(",@", vals)})";

                        if (1 > await cmd.ExecuteNonQueryAsync(cancellationToken))
                        {
                            throw new ApplicationException($"Failed to create TypeDef {name}/{implKey}");
                        }
                    }
                    if (null != mixins && mixins.Any())
                    {
                        using (var cmd = ta.Connection!.CreateCommand())
                        {
                            var idParam = _profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grain.Id);
                            cmd.Parameters.Add(idParam);
                            string rows = PrepareTypeDefMixInInsert(mixins, cmd.Parameters, idParam.ParameterName);
                            cmd.CommandText = $"{GrainTypeDefConfig<TDialect>.SQLInsertTypeDefMixin} ({GrainTypeDefDefaults.MixinExtFieldDerivedType}, {GrainTypeDefDefaults.MixinExtFieldBaseType}) VALUES {rows}";

                            if (1 > await cmd.ExecuteNonQueryAsync(cancellationToken))
                            {
                                throw new ApplicationException($"Failed to store mixins for TypeDef {name}/{implKey}");
                            }
                        }
                    }
                }
                return result;
            }, cancellationToken);
            return result;
        }

        public int StoreGrainTypeDefs(IEnumerable<IGrainTypeDef> typedefs)
        {
            return StoreGrainTypeDefsAsync(typedefs).Result;
        }

        public async Task<int> StoreGrainTypeDefsAsync(IEnumerable<IGrainTypeDef> typedefs, CancellationToken cancellationToken = default)
        {
            var grainsMod = typedefs.Where(g => 0 < g.GetDirtyFields<IGrainBase>().Count);
            var grainsModL = typedefs.Where(g => g is IGrainLocalized gl && 0 < gl.GetDirtyFields<IGrainLocalized>().Count).Select(g => (IGrainLocalized)g);
            var typesMod = typedefs.Where(t => 0 < t.GetDirtyFields<IGrainTypeDef>().Count);
            if (!grainsMod.Any() && !grainsModL.Any() && !typesMod.Any())
            {
                return -1;
            }
            CheckProfile();
            var result = 0;
            return await WrapInTransaction(result, async (ta) =>
            {
                result = await StoreGrainsInTA(grainsMod, result, ta, cancellationToken: cancellationToken);
                result = await StoreLocalizedGrainsInTA(grainsModL, result, ta, cancellationToken);
                return await StoreGrainTypeDefTiersInTA(ta, typesMod, result, cancellationToken);

            }, cancellationToken);
        }

        public IGrainBase? GetOrCreateTypeDefDefaults(IIdentifiable typeDef)
        {
            return GetOrCreateTypeDefDefaultsAsync(typeDef).Result;
        }

        public async Task<IGrainBase?> GetOrCreateTypeDefDefaultsAsync(IIdentifiable typeDef, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            if (!await _accessService.VerfifyAccessAsync(new[] { typeDef }, GrainAccessFlag.Read, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.Read);
            }
            IGrainBase? result = null;
            return await WrapInTransaction(result, async (ta) =>
            {
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    var cols = new string[]
                    {
                        MapGrainBaseColumn(nameof(IGrainBase.Id)),
                        MapGrainBaseColumn(nameof(IGrainBase.Name)),
                        MapGrainBaseColumn(nameof(IGrainBase.TypeDefId)),
                        MapGrainBaseColumn(nameof(IGrainBase.ParentId))
                    };
                    var vals = new string[]
                    {
                        GrainBaseConfig.ParamName,
                        GrainBaseConfig.ParamParentId,
                        GrainBaseConfig.ParamParentId
                    };
                    // INSERT will be ignored by trigger if defaults already exist
                    cmd.CommandText = $"{GrainBaseConfig.SQLInsert}({string.Join(", ", cols)}) VALUES({EngineSpec<TDialect>.Dialect.GuidGen}, @{string.Join(", @", vals)}); {GrainBaseConfig.SQLSelect}{cols[2]} = {cols[3]} AND {cols[2]} = @{vals[1]}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(vals[0], "__defaults__"));
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(vals[1], typeDef.Id));

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            result = new GrainBase(new GrainExtendedDataAdapter(rs, GrainExtendedDataAdapter.ExtensionColumn.Type | GrainExtendedDataAdapter.ExtensionColumn.Path));
                            if (typeDef is IGrainTypeDef typeDefResolved)
                            {
                                typeDefResolved.DefaultInstance = result;
                                typeDefResolved.GetDirtyFields<IGrainTypeDef>().Remove(nameof(IGrainTypeDef.DefaultInstance));
                            }
                        }
                    }
                }
                return result;
            }, cancellationToken);
        }
        #endregion

        #region PropDefManagement
        public IGrainPropDefLocalized? GetPropDef(Guid id, CultureInfo? culture = null)
        {
            return GetPropDefAsync(id, culture).Result;
        }

        public async Task<IGrainPropDefLocalized?> GetPropDefAsync(Guid id, CultureInfo? culture = null, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            using (var conn = _profile.Connection)
            {
                await conn.OpenAsync(cancellationToken);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"{GrainPropDefConfig<TDialect>.SQLSelectPropDefByAclLocalized}g.{MapGrainBaseColumn(nameof(IGrainBase.Id))} = @{GeneralEntityDefaults.ParamId}";
                    _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, (await _accessService.GetContextPrimaryRoleAsync(cancellationToken)).Id);
                    _profile.ParameterFactory.AddParametersForCultureLayer(cmd.Parameters, culture);
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, id));
                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("GetPropDefAsync id={id}, HasRows={hasRows}", id.ToString("D"), rs.HasRows);
                        }
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            return new GrainPropDef(new GrainPropDefDataAdapter(rs));
                        }
                    }
                }
            }
            return null;
        }

        public IGrainPropDef? CreatePropDef(string name, IIdentifiable typeContainer, TraitValueType valueType = TraitValueType.Text, int cardinalityMin = 1, int cardinalityMax = 1)
        {
            return CreatePropDefAsync(name, typeContainer, valueType, cardinalityMin, cardinalityMax).Result;
        }

        public async Task<IGrainPropDef?> CreatePropDefAsync(string name, IIdentifiable typeContainer, TraitValueType valueType = TraitValueType.Text, int cardinalityMin = 1, int cardinalityMax = 1, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            IGrainPropDef? result = null;
            await WrapInTransaction(result, async (ta) =>
            {
                var grain = await CreateGrainInTA(name, typeContainer, (Identifiable)SchemaDefaults.PropDefTypeDefID, ta, cancellationToken: cancellationToken);
                if (null != grain)
                {
                    result = new GrainPropDef(grain)
                    {
                        ValueType = valueType,
                        CardinalityMin = cardinalityMin,
                        CardinalityMax = cardinalityMax
                    };
                    result.GetDirtyFields<IGrainPropDef>().Clear();

                    using (var cmd = ta.Connection!.CreateCommand())
                    {
                        var cols = new List<string>() { GeneralEntityDefaults.FieldBaseId };
                        var vals = new List<string>() { GeneralEntityDefaults.ParamId };
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grain.Id));
                        if (TraitValueType.Text != valueType)
                        {
                            cols.Add(MapPropDefColumn(nameof(IGrainPropDef.ValueType)));
                            vals.Add(GrainPropDefDefaults.ParamValueType);
                            cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainPropDefDefaults.ParamValueType, TraitValueFactory.GetValueTypeAsString(result.ValueType)));
                        }
                        if (0 == cardinalityMin || 1 < cardinalityMin)
                        {
                            cols.Add(MapPropDefColumn(nameof(IGrainPropDef.CardinalityMin)));
                            vals.Add(GrainPropDefDefaults.ParamCardinalityMin);
                            cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainPropDefDefaults.ParamCardinalityMin, cardinalityMin));
                        }
                        if (-1 == cardinalityMax || 1 < cardinalityMax)
                        {
                            cols.Add(MapPropDefColumn(nameof(IGrainPropDef.CardinalityMax)));
                            vals.Add(GrainPropDefDefaults.ParamCardinalityMax);
                            cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainPropDefDefaults.ParamCardinalityMax, cardinalityMax));
                        }

                        cmd.CommandText = $"{GrainPropDefConfig<TDialect>.SQLInsertPropDef} ({string.Join(",", cols)}) VALUES (@{string.Join(",@", vals)})";

                        if (1 > await cmd.ExecuteNonQueryAsync(cancellationToken))
                        {
                            throw new ApplicationException($"Failed to create PropDef {name}");
                        }
                    }

                }
                return result;
            }, cancellationToken);
            return result;
        }

        public int StoreGrainPropDefs(IEnumerable<IGrainPropDef> propdefs)
        {
            return StoreGrainPropDefsAsync(propdefs).Result;
        }

        public async Task<int> StoreGrainPropDefsAsync(IEnumerable<IGrainPropDef> propdefs, CancellationToken cancellationToken = default)
        {
            var grainsMod = propdefs.Where(g => 0 < g.GetDirtyFields<IGrainBase>().Count);
            var grainsModL = propdefs.Where(g => g is IGrainLocalized gl && 0 < gl.GetDirtyFields<IGrainLocalized>().Count).Select(g => (IGrainLocalized)g);
            var propsMod = propdefs.Where(t => 0 < t.GetDirtyFields<IGrainPropDef>().Count);
            if (!grainsMod.Any() && !grainsModL.Any() && !propsMod.Any())
            {
                return -1;
            }
            CheckProfile();
            if (!await _accessService.VerfifyAccessAsync(propdefs, GrainAccessFlag.Write, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.Write);
            }
            var result = 0;
            return await WrapInTransaction(result, async (ta) =>
            {
                result = await StoreGrainsInTA(grainsMod, result, ta, true, cancellationToken);
                result = await StoreLocalizedGrainsInTA(grainsModL, result, ta, cancellationToken);
                return await StoreGrainPropDefTiersInTA(ta, propsMod, result, cancellationToken);
            }, cancellationToken);
        }

        public IEnumerable<IGrainPropDefLocalized> GetTypeDefProperties(IIdentifiable typedef, CultureInfo? culture = null)
        {
            return GetTypeDefPropertiesAsync(typedef, culture).Result;
        }

        public async Task<IEnumerable<IGrainPropDefLocalized>> GetTypeDefPropertiesAsync(IIdentifiable typedef, CultureInfo? culture = null, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            using (var conn = _profile.Connection)
            {
                await conn.OpenAsync(cancellationToken);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"{GrainPropDefConfig<TDialect>.SQLSelectPropDefByAclLocalizedNC}{GrainPropDefConfig<TDialect>.SQLJoinByTypeDefWithInheritance} ORDER BY g.parent_sort_key, g.parent_name";
                    _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, (await _accessService.GetContextPrimaryRoleAsync(cancellationToken)).Id);
                    _profile.ParameterFactory.AddParametersForCultureLayer(cmd.Parameters, culture);
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainTypeDefDefaults.ParamTypeDefId, typedef.Id));
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainTypeDefDefaults.ParamTypeDefPath, $"%{typedef.Id:D}/%"));

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("GetTypeDefPropertiesAsync rs.HasRows={hasRows}, rs.FieldCount={fieldCount}, cmd={commadText}", rs.HasRows, rs.FieldCount, cmd.CommandText);
                        }
                        return await EnumGrainDataReader<IGrainPropDefLocalized, GrainPropDef, GrainPropDefDataAdapter>(rs, cancellationToken: cancellationToken).ToListAsync(cancellationToken);
                    }
                }
            }
        }
        #endregion

        #region Helper Methods

        protected async Task<int> StoreGrainTypeDefTiersInTA(DbTransaction ta, IEnumerable<IGrainTypeDef> typedefs, int result = 0, CancellationToken cancellationToken = default)
        {
            if (typedefs.Any())
            {
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    var mixInField = nameof(IGrainTypeDef.MixIns);
                    foreach (var typedef in typedefs)
                    {
                        var hasMixIns = typedef.GetDirtyFields<IGrainTypeDef>().Contains(mixInField);
                        if (!hasMixIns || 1 < typedef.GetDirtyFields<IGrainTypeDef>().Count)
                        {
                            typedef.GetDirtyFields<IGrainTypeDef>().Remove(mixInField);
                            cmd.Parameters.Clear();
                            cmd.CommandText = GrainTypeDefConfig<TDialect>.SQLUpdateTypeDef;
                            cmd.CommandText += _profile.ParameterFactory.PrepareDirtyFieldsUpdate<GrainTypeDefDataAdapter, IGrainTypeDef>(cmd.Parameters, typedef);
                            cmd.CommandText += $" WHERE {GeneralEntityDefaults.FieldBaseId} = @{GeneralEntityDefaults.ParamId}";

                            cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, typedef.Id));

                            var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                            result += affected;
                            if (0 < affected)
                            {
                                typedef.GetDirtyFields<IGrainTypeDef>().Clear();
                                if (hasMixIns)
                                {
                                    typedef.GetDirtyFields<IGrainTypeDef>().Add(mixInField);
                                }
                            }
                        }

                        if (hasMixIns)
                        {
                            cmd.Parameters.Clear();
                            var idParam = _profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, typedef.Id);
                            cmd.Parameters.Add(idParam);

                            cmd.CommandText = $"{GrainTypeDefConfig<TDialect>.SQLDeleteTypeDefMixin}{GrainTypeDefDefaults.MixinExtFieldDerivedType} = @{idParam.ParameterName}";
                            result += await cmd.ExecuteNonQueryAsync(cancellationToken);

                            if (typedef.MixIns.Any())
                            {
                                string rows = PrepareTypeDefMixInInsert(typedef.MixIns, cmd.Parameters, idParam.ParameterName);
                                cmd.CommandText = $"{GrainTypeDefConfig<TDialect>.SQLInsertTypeDefMixin}({GrainTypeDefDefaults.MixinExtFieldDerivedType}, {GrainTypeDefDefaults.MixinExtFieldBaseType}) VALUES {rows}";

                                var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                                result += affected;
                                if (0 < affected)
                                {
                                    typedef.GetDirtyFields<IGrainTypeDef>().Clear();
                                }
                            }
                            else
                            {
                                typedef.GetDirtyFields<IGrainTypeDef>().Clear();
                            }
                        }
                    }
                }
            }
            return result;
        }

        protected async Task<int> StoreGrainPropDefTiersInTA(DbTransaction ta, IEnumerable<IGrainPropDef> propdefs, int result = 0, CancellationToken cancellationToken = default)
        {
            if (propdefs.Any())
            {
                var valueMapper = new GrainPropDefDataAdapter.FieldValueMapper();
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    foreach (var prop in propdefs)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = GrainPropDefConfig<TDialect>.SQLUpdatePropDef;
                        cmd.CommandText += _profile.ParameterFactory.PrepareDirtyFieldsUpdate<GrainPropDefDataAdapter, IGrainPropDef>(cmd.Parameters, prop, null, valueMapper);
                        cmd.CommandText += $" WHERE {GeneralEntityDefaults.FieldBaseId} = @{GeneralEntityDefaults.FieldId}";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, prop.Id));

                        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                        result += affected;
                        if (0 < affected)
                        {
                            prop.GetDirtyFields<IGrainPropDef>().Clear();
                        }
                    }
                }
            }
            return result;
        }

        protected async Task<IGrainTypeDef> GetTypeDefMixins(IGrainTypeDef typeDef, CancellationToken cancellationToken = default)
        {
            var mixins = await GetTypeDefMixedInTypeIds(typeDef.Id, cancellationToken);
            typeDef.ReplaceMixIns(mixins.Select(x => (Identifiable) x));
            return typeDef;
        }

        protected async Task<ISet<Guid>> GetTypeDefMixedInTypeIds(Guid typeDefId, CancellationToken cancellationToken = default)
        {
            var result = new HashSet<Guid>();
            return await ExecuteOnConnection(result, async (cmd) =>
            {
                using (cmd)
                {
                    cmd.CommandText = $"{GrainTypeDefConfig<TDialect>.SQLSelectTypeDefMixinAnc}{GrainTypeDefDefaults.MixinExtFieldStart} = @{GeneralEntityDefaults.ParamId}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, typeDefId));

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        var ord = rs.GetOrdinal(GrainTypeDefDefaults.MixinExtFieldBaseType);
                        while (await rs.ReadAsync(cancellationToken))
                        {
                            result.Add(rs.GetGuid(ord));
                        }
                    }
                }
                return result;
            }, cancellationToken);
        }

        protected string PrepareTypeDefMixInInsert(IEnumerable<IIdentifiable>? mixins, DbParameterCollection parameters, string idParamName)
        {
            if (null == mixins || !mixins.Any())
            {
                return string.Empty;
            }
            var i = 0;
            return mixins.Aggregate(string.Empty, (aggr, elm) =>
            {
                var param = _profile.ParameterFactory.Create($"mixinId{i++}", elm.Id);
                parameters.Add(param);
                if (0 < aggr.Length)
                {
                    aggr += ", ";
                }
                return aggr + $"(@{idParamName}, @{param.ParameterName})";
            });
        }

        protected static string MapTypeDefColumn(string fieldName)
        {
            return AbstractDataAdapter.GetAdapterColumnName<GrainTypeDefDataAdapter>(fieldName);
        }

        protected static string MapPropDefColumn(string fieldName)
        {
            return AbstractDataAdapter.GetAdapterColumnName<GrainPropDefDataAdapter>(fieldName);
        }
        #endregion
    }
}
