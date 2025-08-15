using System.Data.Common;
using System.Globalization;
using CraftedSolutions.MarBasBrokerSQLCommon;
using CraftedSolutions.MarBasBrokerSQLCommon.Access;
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

namespace CraftedSolutions.MarBasBrokerSQLCommon.BrokerImpl
{
    public abstract class CloningBroker<TDialect>
        : FileManagementBroker<TDialect>, ICloningBroker, IAsyncCloningBroker
        where TDialect : ISQLDialect, new()
    {
        protected CloningBroker(IBrokerProfile profile, ILogger logger) : base(profile, logger)
        {
        }

        protected CloningBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger logger) : base(profile, context, accessService, logger)
        {
        }

        public IGrainBase? CloneGrain(IIdentifiable grain, IIdentifiable? newParent = null, GrainCloneDepth depth = GrainCloneDepth.Self, bool copyAcl = false)
        {
            return CloneGrainAsync(grain, newParent, depth, copyAcl).Result;
        }

        public async Task<IGrainBase?> CloneGrainAsync(IIdentifiable grain, IIdentifiable? newParent = null, GrainCloneDepth depth = GrainCloneDepth.Self, bool copyAcl = false, CancellationToken cancellationToken = default)
        {
            return await CloneGrainInternal(grain, newParent, depth, copyAcl, 0, cancellationToken);
        }

        protected async Task<IGrainBase?> CloneGrainInternal(IIdentifiable grain, IIdentifiable? newParent = null, GrainCloneDepth depth = GrainCloneDepth.Self, bool copyAcl = false, int currentDepth = 0, CancellationToken cancellationToken = default)
        {
            await CheckProfile(cancellationToken);
            if (!await _accessService.VerfifyAccessAsync(new[] { grain }, GrainAccessFlag.Read, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.Read);
            }
            if (!await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.WriteAcl, cancellationToken: cancellationToken))
            {
                throw new UnauthorizedAccessException("Not entitled to create ACL");
            }

            var srcGrain = await CastIdentifiableToGrainBase(grain, cancellationToken);
            if (null == srcGrain || null == srcGrain.ParentId)
            {
                throw new InvalidOperationException("Source grain is not copyable");
            }

            IGrainBase? result = null;
            if (0 < depth || GrainCloneDepth.Self == (GrainCloneDepth.Self & depth))
            {
                await WrapInTransaction(result, async (ta) =>
                {

                    result = await CloneGrainBaseInTA(srcGrain, newParent, ta, cancellationToken);
                    if (null != result)
                    {
                        var affected = 0;
                        if (copyAcl)
                        {
                            affected = await CloneGrainAclInTA(srcGrain.Id, result.Id, ta, cancellationToken);
                            if (_logger.IsEnabled(LogLevel.Debug))
                            {
                                _logger.LogDebug("Cloned {affected} ACL from {sourceId} to {resultId}", affected, srcGrain.Id, result.Id);
                            }
                        }
                        else if (0 == currentDepth)
                        {
                            if (null == await CreateDefaultAclInTA(result.Id, ta, cancellationToken))
                            {
                                throw new ApplicationException($"Failed to create ACL for {result.Id}");
                            }
                        }

                        if (null == srcGrain.TypeDefId)
                        {
                            if (1 > await CloneGrainTypeDetailsInTA(srcGrain.Id, result.Id, new[] { MapTypeDefColumn(nameof(IGrainTypeDef.Impl)) },
                                GrainTypeDefConfig<TDialect>.SQLInsertTypeDef, GrainTypeDefDefaults.DataSourceTypeDef, ta, cancellationToken))
                            {
                                throw new ApplicationException($"Failed to clone TypeDef details from {srcGrain.Id} to {result.Id}");
                            }
                            using (var cmd = ta.Connection!.CreateCommand())
                            {
                                cmd.CommandText = $"{GrainTypeDefConfig<TDialect>.SQLInsertTypeDefMixin}({GrainTypeDefDefaults.MixinExtFieldDerivedType}, {GrainTypeDefDefaults.MixinExtFieldBaseType}) SELECT @{GrainTypeDefDefaults.ParamTypeDefId}, {GrainTypeDefDefaults.MixinExtFieldBaseType} FROM {GrainTypeDefDefaults.DataSourceTypeDefMixin} WHERE {GrainTypeDefDefaults.MixinExtFieldDerivedType} = @{GrainTypeDefDefaults.MixinExtFieldDerivedType}";
                                cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainTypeDefDefaults.ParamTypeDefId, result.Id));
                                cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainTypeDefDefaults.MixinExtFieldDerivedType, srcGrain.Id));
                                affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                                if (_logger.IsEnabled(LogLevel.Debug))
                                {
                                    _logger.LogDebug("Cloned {affected} base mixins from {sourceId} to {resultId}", affected, srcGrain.Id, result.Id);
                                }
                            }
                        }
                        else if (await IsGrainInstanceOfAsync(srcGrain, (Identifiable)SchemaDefaults.PropDefTypeDefID))
                        {
                            if (1 > await CloneGrainTypeDetailsInTA(srcGrain.Id, result.Id, new[]
                                {
                                MapPropDefColumn(nameof(IGrainPropDef.ValueType)),
                                MapPropDefColumn(nameof(IGrainPropDef.CardinalityMin)),
                                MapPropDefColumn(nameof(IGrainPropDef.CardinalityMax)),
                                MapPropDefColumn(nameof(IGrainPropDef.Versionable)),
                                MapPropDefColumn(nameof(IGrainPropDef.Localizable)),
                                MapPropDefColumn(nameof(IGrainPropDef.ValueConstraint))
                            },
                                GrainPropDefConfig<TDialect>.SQLInsertPropDef, GrainPropDefDefaults.DataSourcePropDef, ta, cancellationToken))
                            {
                                throw new ApplicationException($"Failed to clone PropDef details from {srcGrain.Id} to {result.Id}");
                            }
                        }
                        else if (await IsGrainInstanceOfAsync(srcGrain, (Identifiable)SchemaDefaults.FileTypeDefID))
                        {
                            if (1 > await CloneFileDetailsInTA(srcGrain.Id, result.Id, ta, cancellationToken))
                            {
                                throw new ApplicationException($"Failed to clone File details from {srcGrain.Id} to {result.Id}");
                            }
                            await CloneFileBlobInTA(srcGrain.Id, result.Id, ta, cancellationToken);
                        }

                        affected = await CloneGrainLabelsInTA(srcGrain.Id, result.Id, ta, cancellationToken);
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("Cloned {affected} labels from {sourceId} to {resultId}", affected, srcGrain.Id, result.Id);
                        }

                        affected = await CloneGrainTraitsInTA(srcGrain.Id, result.Id, ta, cancellationToken);
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("Cloned {affected} traits from {sourceId} to {resultId}", affected, srcGrain.Id, result.Id);
                        }
                    }

                    return result;
                }, cancellationToken);
            }
            else
            {
                result = srcGrain;
            }

            if (GrainCloneDepth.Infinite == (GrainCloneDepth.Infinite & depth) || 2 > currentDepth && GrainCloneDepth.Immediate == (GrainCloneDepth.Infinite & depth))
            {
                var children = new List<IGrainBase>();
                await ExecuteOnConnection(children, async (cmd) =>
                {
                    cmd.CommandText = $"SELECT * FROM {GrainBaseConfig.DataSource} WHERE {MapGrainBaseColumn(nameof(IGrainBase.ParentId))} = @{GeneralEntityDefaults.ParamId}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grain.Id));

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await rs.ReadAsync(cancellationToken))
                        {
                            children.Add(new GrainBase(new GrainExtendedDataAdapter(rs, GrainExtendedDataAdapter.ExtensionColumn.None)));
                        }
                    }

                    return children;
                }, cancellationToken);

                foreach (var child in children)
                {
                    if (null == await CloneGrainInternal(child, result, depth, copyAcl, currentDepth + 1, cancellationToken) && _logger.IsEnabled(LogLevel.Warning))
                    {
                        _logger.LogWarning("Failed to clone child {childId}", child.Id);
                    }
                }
            }

            return result;

        }

        protected async Task<GrainBase?> CloneGrainBaseInTA(IGrainBase sourceGrain, IIdentifiable? newParent, DbTransaction ta, CancellationToken cancellationToken)
        {
            var tgtName = sourceGrain.Name;
            if (null == newParent || sourceGrain.ParentId == newParent.Id)
            {
                var d = DateTime.UtcNow;
                tgtName = $"Copy of {tgtName} {d.ToString("yyyyMMddhhmm", CultureInfo.InvariantCulture)}{RandomSeed.Next(1, 10000)}";
            }

            var tgtParentId = newParent?.Id ?? sourceGrain.ParentId;
            if (null == tgtParentId || !await _accessService.VerfifyAccessAsync(new[] { (Identifiable)tgtParentId }, GrainAccessFlag.CreateSubelement, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.CreateSubelement);
            }

            using (var cmd = ta.Connection!.CreateCommand())
            {
                var cols = new[]
                {
                    MapGrainBaseColumn(nameof(IGrainBase.TypeDefId)),
                    MapGrainBaseColumn(nameof(IGrainBase.SortKey)),
                    MapGrainBaseColumn(nameof(IGrainBase.XAttrs)),
                    MapGrainBaseColumn(nameof(IGrainBase.Revision)),
                    MapGrainBaseColumn(nameof(IGrainBase.CTime)),
                    MapGrainBaseColumn(nameof(IGrainBase.MTime))
                };
                cmd.CommandText = $"{GrainBaseConfig.SQLInsert}({GeneralEntityDefaults.FieldId}, {MapGrainBaseColumn(nameof(IGrainBase.Name))}, {MapGrainBaseColumn(nameof(IGrainBase.ParentId))}, {MapGrainBaseColumn(nameof(IGrainBase.Owner))}, {string.Join(", ", cols)}) SELECT {EngineSpec<TDialect>.Dialect.GuidGen}, @{GrainBaseConfig.ParamName}, @{GrainBaseConfig.ParamParentId}, @{GrainBaseConfig.ParamOwner}, {string.Join(", ", cols)} FROM {GrainBaseConfig.DataSource} WHERE {GeneralEntityDefaults.FieldId} = @{GeneralEntityDefaults.ParamId}{EngineSpec<TDialect>.Dialect.ReturnFromInsert}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, sourceGrain.Id));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainBaseConfig.ParamName, GrainBase.SanitizeName(tgtName)));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainBaseConfig.ParamParentId, tgtParentId));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainBaseConfig.ParamOwner, _context.User.Identity?.Name ?? SchemaDefaults.SystemUserName));

                using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (await rs.ReadAsync(cancellationToken))
                    {
                        return new GrainBase(new GrainExtendedDataAdapter(rs, GrainExtendedDataAdapter.ExtensionColumn.None), sourceGrain);
                    }
                }
            }
            return null;
        }

        protected async Task<int> CloneGrainAclInTA(Guid oldGrainId, Guid newGrainId, DbTransaction ta, CancellationToken cancellationToken)
        {
            using (var cmd = ta.Connection!.CreateCommand())
            {
                var cols = new[]
                {
                    MapAclColumn(nameof(ISchemaAclEntry.RoleId)),
                    MapAclColumn(nameof(ISchemaAclEntry.PermissionMask)),
                    MapAclColumn(nameof(ISchemaAclEntry.RestrictionMask)),
                    MapAclColumn(nameof(ISchemaAclEntry.Inherit))
                };
                cmd.CommandText = $"{AclConfig<TDialect>.SQLInsertAcl}({GeneralEntityDefaults.FieldGrainId}, {string.Join(",", cols)}) SELECT @{GeneralEntityDefaults.ParamId}, {string.Join(", ", cols)} FROM {AclDefaults.DataSourceAcl} WHERE {GeneralEntityDefaults.FieldGrainId} = @{GeneralEntityDefaults.ParamGrainId}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, oldGrainId));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, newGrainId));

                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        protected async Task<ISchemaAclEntry?> CreateDefaultAclInTA(Guid grainId, DbTransaction ta, CancellationToken cancellationToken)
        {
            using (var cmd = ta.Connection!.CreateCommand())
            {
                var cols = new[]
                {
                    MapAclColumn(nameof(ISchemaAclEntry.RoleId)),
                    MapAclColumn(nameof(ISchemaAclEntry.GrainId)),
                    MapAclColumn(nameof(ISchemaAclEntry.PermissionMask))
                };
                var vals = new[]
                {
                    AclDefaults.ParamRoleId,
                    AclDefaults.ParamGrainId,
                    AclDefaults.ParamPermissionMask
                };
                cmd.CommandText = $"{AclConfig<TDialect>.SQLInsertAcl}({string.Join(",", cols)}) VALUES (@{string.Join(",@", vals)}){EngineSpec<TDialect>.Dialect.ReturnFromInsert}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(vals[0], await _accessService.GetContextPrimaryRoleAsync(cancellationToken)));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(vals[1], grainId));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(vals[2], GrainAccessFlag.Full));

                using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (await rs.ReadAsync(cancellationToken))
                    {
                        return new SchemaAclEntry(new AclDataAdapter(rs, AclDataAdapter.ExtensionColumn.None));
                    }
                }
            }
            return null;
        }

        protected async Task<int> CloneGrainLabelsInTA(Guid oldGrainId, Guid newGrainId, DbTransaction ta, CancellationToken cancellationToken)
        {
            using (var cmd = ta.Connection!.CreateCommand())
            {
                var cols = new[]
                {
                    GeneralEntityDefaults.FieldLangCode,
                    GrainLocalizedDefaults.FieldLabel
                };
                cmd.CommandText = $"{GrainLocalizedConfig<TDialect>.SQLInsertLabel}({GeneralEntityDefaults.FieldGrainId}, {string.Join(", ", cols)}) SELECT @{GeneralEntityDefaults.ParamId}, {string.Join(", ", cols)} FROM {GrainLocalizedDefaults.DataSourceLabel} WHERE {GeneralEntityDefaults.FieldGrainId} = @{GeneralEntityDefaults.ParamGrainId}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, oldGrainId));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, newGrainId));

                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        protected async Task<int> CloneGrainTraitsInTA(Guid oldGrainId, Guid newGrainId, DbTransaction ta, CancellationToken cancellationToken)
        {
            using (var cmd = ta.Connection!.CreateCommand())
            {
                var cols = new List<string>
                {
                    MapTraitColumn(nameof(ITraitBase.PropDefId)),
                    MapTraitColumn(nameof(ITraitBase.Culture)),
                    MapTraitColumn(nameof(ITraitBase.Revision)),
                    MapTraitColumn(nameof(ITraitBase.Ord))
                };
                foreach (var valType in Enum.GetValues<TraitValueType>())
                {
                    if (valType != TraitValueType.File)
                    {
                        cols.Add(TraitBaseDataAdapter.GetValueColumn(valType));
                    }
                }
                cmd.CommandText = $"{TraitBaseConfig<TDialect>.SQLInsert}({GeneralEntityDefaults.FieldId}, {GeneralEntityDefaults.FieldGrainId}, {string.Join(", ", cols)}) SELECT {EngineSpec<TDialect>.Dialect.GuidGenPerRow("result <> id")}, @{GeneralEntityDefaults.ParamId}, {string.Join(", ", cols)} FROM {TraitBaseDefaults.DataSource} WHERE {GeneralEntityDefaults.FieldGrainId} = @{GeneralEntityDefaults.ParamGrainId}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, oldGrainId));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, newGrainId));

                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        protected async Task<int> CloneGrainTypeDetailsInTA(Guid oldBaseId, Guid newBaseId, IEnumerable<string> cols, string insertPfx, string detailTable, DbTransaction ta, CancellationToken cancellationToken)
        {
            using (var cmd = ta.Connection!.CreateCommand())
            {
                cmd.CommandText = $"{insertPfx}({GeneralEntityDefaults.FieldBaseId}, {string.Join(", ", cols)}) SELECT @{GeneralEntityDefaults.ParamId}, {string.Join(", ", cols)} FROM {detailTable} WHERE {GeneralEntityDefaults.FieldBaseId} = @{GeneralEntityDefaults.ParamGrainId}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, oldBaseId));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, newBaseId));

                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        protected async Task<int> CloneFileDetailsInTA(Guid oldBaseId, Guid newBaseId, DbTransaction ta, CancellationToken cancellationToken)
        {
            using (var cmd = ta.Connection!.CreateCommand())
            {
                var cols = new[]
                {
                    MapFileColumn(nameof(IGrainFile.MimeType)),
                    MapFileColumn(nameof(IGrainFile.Size))
                };
                cmd.CommandText = $"{GrainFileConfig<TDialect>.SQLInsertFile}({GeneralEntityDefaults.FieldBaseId}, {MapFileColumn(nameof(IGrainFile.Content))}, {string.Join(", ", cols)}) SELECT @{GeneralEntityDefaults.ParamId}, {EngineSpec<TDialect>.Dialect.NewBlobContent(MapFileColumn(nameof(IGrainFile.Size)))}, {string.Join(", ", cols)} FROM {GrainFileDefaults.DataSourceFile} WHERE {GeneralEntityDefaults.FieldBaseId} = @{GeneralEntityDefaults.ParamGrainId}";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamGrainId, oldBaseId));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, newBaseId));

                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        protected abstract Task CloneFileBlobInTA(Guid sourceFileId, Guid targetFileId, DbTransaction ta, CancellationToken cancellationToken);

        protected static string MapAclColumn(string fieldName)
        {
            return AbstractDataAdapter.GetAdapterColumnName<AclDataAdapter>(fieldName);
        }
    }
}
