using MarBasBrokerSQLCommon.Access;
using MarBasCommon;
using MarBasSchema;
using MarBasSchema.Access;
using MarBasSchema.Broker;
using Microsoft.Extensions.Logging;

namespace MarBasBrokerSQLCommon.BrokerImpl
{
    public abstract class AclManagementBroker<TDialect>
        : RoleManagementBroker<TDialect>, IAclManagementBroker, IAsyncAclManagementBroker
         where TDialect : ISQLDialect, new()
    {
        protected AclManagementBroker(IBrokerProfile profile, ILogger logger) : base(profile, logger)
        {
        }

        protected AclManagementBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger logger) : base(profile, context, accessService, logger)
        {
        }

        public ISchemaAclEntry? GetAclEntry(IIdentifiable role, IIdentifiable grain)
        {
            return GetAclEntryAsync(role, grain).Result;
        }

        public async Task<ISchemaAclEntry?> GetAclEntryAsync(IIdentifiable role, IIdentifiable grain, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            if (role.Id != (await _accessService.GetContextPrimaryRoleAsync(cancellationToken)).Id && !await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.ReadAcl, false, cancellationToken))
            {
                return null;
            }
            if (!await _accessService.VerfifyAccessAsync(new[] { grain }, GrainAccessFlag.Read, cancellationToken))
            {
                return null;
            }
            using (var conn = _profile.Connection)
            {
                await conn.OpenAsync(cancellationToken);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"{AclConfig<TDialect>.SQLSelectAcl}{MapAclColumn(nameof(ISchemaAclEntry.RoleId))} = @{AclDefaults.ParamRoleId}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(AclDefaults.ParamRoleId, role.Id));
                    cmd.CommandText += $" AND {MapAclColumn(nameof(ISchemaAclEntry.GrainId))}";
                    cmd.CommandText += $" = @{AclDefaults.ParamGrainId}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(AclDefaults.ParamGrainId, grain.Id));
                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            return new SchemaAclEntry(new AclDataAdapter(rs, AclDataAdapter.ExtensionColumn.None));
                        }
                    }
                }
            }
            return null;
        }

        public ISchemaAclEntry? CreateAclEntry(IIdentifiable role, IIdentifiable grain, GrainAccessFlag permissionMask = GrainAccessFlag.Read, GrainAccessFlag restrictionMask = GrainAccessFlag.None, bool inherit = true)
        {
            return CreateAclEntryAsync(role, grain, permissionMask, restrictionMask, inherit).Result;
        }

        public async Task<ISchemaAclEntry?> CreateAclEntryAsync(IIdentifiable role, IIdentifiable grain, GrainAccessFlag permissionMask = GrainAccessFlag.Read, GrainAccessFlag restrictionMask = GrainAccessFlag.None, bool inherit = true, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            if (!await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.WriteAcl, cancellationToken: cancellationToken))
            {
                throw new UnauthorizedAccessException("Not entitled to create ACL");
            }
            if (!await _accessService.VerfifyAccessAsync(new[] { grain }, GrainAccessFlag.ModifyAcl, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.ModifyAcl);
            }
            ISchemaAclEntry? result = null;
            result = await WrapInTransaction(result, async (ta) =>
            {
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    var cols = new List<string>
                    {
                        MapAclColumn(nameof(ISchemaAclEntry.RoleId)),
                        MapAclColumn(nameof(ISchemaAclEntry.GrainId))
                    };
                    var vals = new List<string>
                    {
                        AclDefaults.ParamRoleId,
                        GeneralEntityDefaults.ParamGrainId
                    };
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(vals[0], role.Id));
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(vals[1], grain.Id));
                    if (GrainAccessFlag.Read != permissionMask)
                    {
                        cols.Add(MapAclColumn(nameof(ISchemaAclEntry.PermissionMask)));
                        vals.Add(AclDefaults.ParamPermissionMask);
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(vals.Last(), permissionMask));
                    }
                    if (GrainAccessFlag.None != restrictionMask)
                    {
                        cols.Add(MapAclColumn(nameof(ISchemaAclEntry.RestrictionMask)));
                        vals.Add(AclDefaults.ParamRestrictionMask);
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(vals.Last(), restrictionMask));
                    }
                    if (!inherit)
                    {
                        cols.Add(MapAclColumn(nameof(ISchemaAclEntry.Inherit)));
                        vals.Add(AclDefaults.ParamInherit);
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(vals.Last(), inherit));
                    }
                    cmd.CommandText = $"{AclConfig<TDialect>.SQLInsertAcl}({string.Join(",", cols)}) VALUES (@{string.Join(",@", vals)}){EngineSpec<TDialect>.Dialect.ReturnFromInsert}";

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            result = new SchemaAclEntry(new AclDataAdapter(rs, AclDataAdapter.ExtensionColumn.None));
                        }
                    }
                }
                return result;
            }, cancellationToken);
            return result;
        }

        public int DeleteAcl(IEnumerable<IAclEntryRef> acl)
        {
            return DeleteAclAsync(acl).Result;
        }

        public async Task<int> DeleteAclAsync(IEnumerable<IAclEntryRef> acl, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            var requiredEntitlement = RoleEntitlement.DeleteAcl;
            if (acl.Any(x => SchemaDefaults.BuiltInAcl.Contains((x.GrainId, x.RoleId))))
            {
                requiredEntitlement |= RoleEntitlement.DeleteBuiltInElements;
            }
            if (!await _accessService.VerifyRoleEntitlementAsync(requiredEntitlement, cancellationToken: cancellationToken))
            {
                throw new UnauthorizedAccessException("Not entitled to delete ACL");
            }
            if (!await _accessService.VerfifyAccessAsync(acl.Where(x => SchemaDefaults.AnyGrainID != x.GrainId).Select(x => (Identifiable)(x.GrainId)), GrainAccessFlag.ModifyAcl, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.ModifyAcl);
            }
            var result = 0;
            result = await WrapInTransaction(result, async (ta) =>
            {
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    cmd.CommandText = $"{AclConfig<TDialect>.SQLDeleteAcl}{MapAclColumn(nameof(ISchemaAclEntry.RoleId))} = @{AclDefaults.FieldRoleId} AND {MapAclColumn(nameof(ISchemaAclEntry.GrainId))} = @{AclDefaults.ParamGrainId}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(AclDefaults.FieldRoleId, Guid.Empty));
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(AclDefaults.ParamGrainId, Guid.Empty));
                    foreach (var entry in acl)
                    {
                        _profile.ParameterFactory.Update(cmd.Parameters[0], entry.RoleId);
                        _profile.ParameterFactory.Update(cmd.Parameters[1], entry.GrainId);

                        result += await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
                return result;
            }, cancellationToken);
            return result;
        }

        public int StoreAcl(IEnumerable<ISchemaAclEntry> acl)
        {
            return StoreAclAsync(acl).Result;
        }

        public async Task<int> StoreAclAsync(IEnumerable<ISchemaAclEntry> acl, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            if (!await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.WriteAcl, cancellationToken: cancellationToken))
            {
                throw new UnauthorizedAccessException("Not entitled to modify ACL");
            }
            if (!await _accessService.VerfifyAccessAsync(acl.Where(x => SchemaDefaults.AnyGrainID != x.GrainId).Select(x => (Identifiable)(x.GrainId!)), GrainAccessFlag.ModifyAcl, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.ModifyAcl);
            }
            var aclMod = acl.Where(a => 0 < a.GetDirtyFields<ISchemaAclEntry>().Count);
            if (!aclMod.Any())
            {
                return -1;
            }
            var result = 0;
            result = await WrapInTransaction(result, async (ta) =>
            {
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    var roleIdCol = MapAclColumn(nameof(ISchemaAclEntry.RoleId));
                    var grainIdCol = MapAclColumn(nameof(ISchemaAclEntry.GrainId));
                    foreach (var entry in aclMod)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = AclConfig<TDialect>.SQLUpdateAcl;
                        cmd.CommandText += _profile.ParameterFactory.PrepareDirtyFieldsUpdate<AclDataAdapter, ISchemaAclEntry>(cmd.Parameters, entry);
                        cmd.CommandText += $" WHERE {roleIdCol} = @{AclDefaults.ParamRoleId} AND {grainIdCol} = @{AclDefaults.ParamGrainId}";

                        cmd.Parameters.Add(_profile.ParameterFactory.Create(AclDefaults.ParamRoleId, entry.RoleId));
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(AclDefaults.ParamGrainId, entry.GrainId));

                        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                        result += affected;
                        if (0 < affected)
                        {
                            entry.GetDirtyFields<ISchemaRole>().Clear();
                        }
                    }
                }
                return result;
            }, cancellationToken);
            return result;
        }

        public IEnumerable<ISchemaAclEntry> GetEffectiveAcl(IIdentifiable grain)
        {
            return GetEffectiveAclAsync(grain).Result;
        }

        public async Task<IEnumerable<ISchemaAclEntry>> GetEffectiveAclAsync(IIdentifiable grain, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            var result = new List<ISchemaAclEntry>();
            if (!await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.ReadAcl, cancellationToken: cancellationToken))
            {
                return result;
            }
            if (!await _accessService.VerfifyAccessAsync(new[] { grain }, GrainAccessFlag.Read, cancellationToken))
            {
                return result;
            }
            using (var conn = _profile.Connection)
            {
                await conn.OpenAsync(cancellationToken);
                using (var cmd = conn.CreateCommand())
                {
                    var idCol = MapAclColumn(nameof(ISchemaAclEntry.GrainId));
                    var paramAnyGrain = "anyGrainId";
                    cmd.CommandText = $"{AclConfig<TDialect>.SQLSelectAclEffective}({idCol} = @{AclDefaults.ParamGrainId} OR {idCol} = @{paramAnyGrain}) AND {MapAclColumn(nameof(ISchemaAclEntry.RoleId))} IS NOT NULL";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(AclDefaults.ParamGrainId, grain.Id));
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(paramAnyGrain, SchemaDefaults.AnyGrainID));

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await rs.ReadAsync(cancellationToken))
                        {
                            result.Add(new SchemaAclEntry(new AclDataAdapter(rs)));
                        }
                    }
                }
            }
            return result;
        }

        #region Helper Methods

        protected static string MapAclColumn(string fieldName)
        {
            return AbstractDataAdapter.GetAdapterColumnName<AclDataAdapter>(fieldName);
        }
        #endregion
    }
}
