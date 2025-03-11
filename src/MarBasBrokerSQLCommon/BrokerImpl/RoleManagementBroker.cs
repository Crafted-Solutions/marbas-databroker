using CraftedSolutions.MarBasBrokerSQLCommon;
using CraftedSolutions.MarBasBrokerSQLCommon.Access;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Broker;
using CraftedSolutions.MarBasSchema.Event;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasBrokerSQLCommon.BrokerImpl
{
    public abstract class RoleManagementBroker<TDialect>
        : BaseSchemaBroker<TDialect>, IRoleManagementBroker, IAsyncRoleManagementBroker
         where TDialect : ISQLDialect, new()
    {
        protected RoleManagementBroker(IBrokerProfile profile, ILogger logger)
            : base(profile, logger)
        {
        }

        protected RoleManagementBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger logger)
            : base(profile, context, accessService, logger)
        {
        }

        public ISchemaRole? GetRole(Guid id)
        {
            return GetRoleAsync(id).Result;
        }

        public async Task<ISchemaRole?> GetRoleAsync(Guid id, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            if (id != (await _accessService.GetContextPrimaryRoleAsync(cancellationToken)).Id && !await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.ReadRoles, cancellationToken: cancellationToken))
            {
                return null;
            }
            using (var conn = _profile.Connection)
            {
                await conn.OpenAsync(cancellationToken);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"{RoleConfig<TDialect>.SQLSelectRole}{AbstractDataAdapter.GetAdapterColumnName<RoleDataAdapter>(nameof(ISchemaRole.Id))} = @{GeneralEntityDefaults.ParamId}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, id));

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            return new SchemaRole(new RoleDataAdapter(rs));
                        }
                    }
                }
            }
            return null;
        }

        public ISchemaRole? CreateRole(string name, RoleEntitlement entitlement = RoleEntitlement.None)
        {
            return CreateRoleAsync(name, entitlement).Result;
        }

        public async Task<ISchemaRole?> CreateRoleAsync(string name, RoleEntitlement entitlement = RoleEntitlement.None, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            if (!await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.WriteRoles, cancellationToken: cancellationToken))
            {
                throw new UnauthorizedAccessException("Unsufficient entitlement for creating roles");
            }
            ISchemaRole? result = null;
            result = await WrapInTransaction(result, async (ta) =>
            {
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    var cols = new List<string>()
                    {
                        AbstractDataAdapter.GetAdapterColumnName<RoleDataAdapter>(nameof(ISchemaRole.Id)),
                        AbstractDataAdapter.GetAdapterColumnName<RoleDataAdapter>(nameof(ISchemaRole.Name)),
                        AbstractDataAdapter.GetAdapterColumnName<RoleDataAdapter>(nameof(ISchemaRole.Entitlement))
                    };
                    var vals = new List<string>()
                    {
                        RoleDefaults.ParamName,
                        RoleDefaults.ParamEntitlement
                    };

                    cmd.CommandText = $"{RoleConfig<TDialect>.SQLInsertRole}({string.Join(",", cols)}) VALUES ({EngineSpec<TDialect>.Dialect.GuidGen}, @{string.Join(",@", vals)}){EngineSpec<TDialect>.Dialect.ReturnFromInsert}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(vals[0], name));
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(vals[1], entitlement));

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            result = new SchemaRole(new RoleDataAdapter(rs));
                            _profile.DispatchSchemaModified<ISchemaRole>(SchemaModificationType.Create, new[] { result });
                        }
                    }
                }
                return result;
            }, cancellationToken);
            return result;
        }

        public int DeleteRoles(IEnumerable<IIdentifiable> ids)
        {
            return DeleteRolesAsync(ids).Result;
        }

        public async Task<int> DeleteRolesAsync(IEnumerable<IIdentifiable> ids, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            CheckBuiltIns(ids);
            if (!await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.DeleteRoles, cancellationToken: cancellationToken))
            {
                throw new UnauthorizedAccessException("Unsufficient entitlement for deleting roles");
            }
            var result = 0;
            var succeeded = new List<IIdentifiable>();
            result = await WrapInTransaction(result, async (ta) =>
            {
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    cmd.CommandText = $"{RoleConfig<TDialect>.SQLDeleteRole}{AbstractDataAdapter.GetAdapterColumnName<RoleDataAdapter>(nameof(ISchemaRole.Id))} = @{GeneralEntityDefaults.ParamId}";
                    var param = _profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, (Guid?)null);
                    cmd.Parameters.Add(param);

                    foreach (var id in ids)
                    {
                        _profile.ParameterFactory.Update(param, id);
                        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                        result += affected;
                        if (0 < affected)
                        {
                            succeeded.Add(id);
                        }
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("DeleteRolesAsync id={id}, success={success}", cmd.Parameters[0].Value, result);
                        }
                    }

                }
                return result;
            }, cancellationToken);
            if (0 < result)
            {
                _profile.DispatchSchemaModified<ISchemaRole>(SchemaModificationType.Delete, succeeded);
            }
            return result;
        }

        public int StoreRoles(IEnumerable<ISchemaRole> roles)
        {
            return StoreRolesAsync(roles).Result;
        }

        public async Task<int> StoreRolesAsync(IEnumerable<ISchemaRole> roles, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            if (!await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.WriteRoles, cancellationToken: cancellationToken))
            {
                throw new UnauthorizedAccessException("Unsufficient entitlement for modifying roles");
            }
            var rolesMod = roles.Where(r => 0 < r.GetDirtyFields<ISchemaRole>().Count);
            if (!rolesMod.Any())
            {
                return -1;
            }
            var result = 0;
            var succeeded = new List<IIdentifiable>();
            result = await WrapInTransaction(result, async (ta) =>
            {
                var idCol = AbstractDataAdapter.GetAdapterColumnName<RoleDataAdapter>(nameof(ISchemaRole.Id));
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    foreach (var role in rolesMod)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = RoleConfig<TDialect>.SQLUpdateRole;
                        cmd.CommandText += _profile.ParameterFactory.PrepareDirtyFieldsUpdate<RoleDataAdapter, ISchemaRole>(cmd.Parameters, role);
                        cmd.CommandText += $" WHERE {idCol} = @{GeneralEntityDefaults.ParamId}";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, role.Id));

                        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                        result += affected;
                        if (0 < affected)
                        {
                            succeeded.Add(new SchemaRole(role));
                            role.GetDirtyFields<ISchemaRole>().Clear();
                        }
                    }
                }
                return result;
            }, cancellationToken);
            if (0 < result)
            {
                _profile.DispatchSchemaModified<ISchemaRole>(SchemaModificationType.Update, succeeded);
            }
            return result;
        }

        public IEnumerable<ISchemaRole> ListRoles(IEnumerable<IListSortOption<RoleSortField>>? sortOptions = null)
        {
            return ListRolesAsync(sortOptions).Result;
        }

        public async Task<IEnumerable<ISchemaRole>> ListRolesAsync(IEnumerable<IListSortOption<RoleSortField>>? sortOptions = null, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            var result = new List<ISchemaRole>();
            if (!await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.ReadRoles, cancellationToken: cancellationToken))
            {
                return result;
            }
            using (var conn = _profile.Connection)
            {
                await conn.OpenAsync(cancellationToken);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"{RoleConfig<TDialect>.SQLSelect}";
                    cmd.CommandText += PrepareListOrderByClause<RoleSortField, RoleDataAdapter>(sortOptions);

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await rs.ReadAsync(cancellationToken))
                        {
                            result.Add(new SchemaRole(new RoleDataAdapter(rs)));
                        }
                    }
                }
            }
            return result;
        }
    }
}
