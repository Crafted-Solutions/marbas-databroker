using CraftedSolutions.MarBasBrokerSQLCommon;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Broker;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasBrokerSQLCommon.Access
{
    public abstract class SQLAccessService<TDialect>
        : IAccessService, IAsyncAccessService
        where TDialect : ISQLDialect, new()
    {
        protected readonly ISQLBrokerProfile _profile;
        protected readonly ILogger _logger;
        protected readonly IEnumerable<string> _contextRoles;

        protected SQLAccessService(IBrokerContext context, IBrokerProfile profile, ILogger logger)
        {
            _contextRoles = context.UserRoles.ToList();
            _profile = (ISQLBrokerProfile)profile;
            _logger = logger;
        }

        public ISchemaRole GetContextPrimaryRole()
        {
            return GetContextPrimaryRoleAsync().Result;
        }

        public async Task<ISchemaRole> GetContextPrimaryRoleAsync(CancellationToken cancellationToken = default)
        {
            return (await GetContextRolesAsync(cancellationToken)).FirstOrDefault() ?? SchemaRole.Everyone;
        }

        public IEnumerable<ISchemaRole> GetContextRoles()
        {
            var sfxLen = SchemaDefaults.InternalPrincipalSuffix.Length;
            return _profile.SchemaRoles.Where(x => _contextRoles.Contains(x.Name.EndsWith(SchemaDefaults.InternalPrincipalSuffix) ? x.Name.Remove(x.Name.Length - sfxLen) : x.Name));
        }

        public Task<IEnumerable<ISchemaRole>> GetContextRolesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetContextRoles());
        }

        public bool VerfifyAccess(IEnumerable<IIdentifiable> grains, GrainAccessFlag desiredAccess)
        {
            return VerfifyAccessAsync(grains, desiredAccess).Result;
        }

        public async Task<bool> VerfifyAccessAsync(IEnumerable<IIdentifiable> grains, GrainAccessFlag desiredAccess, CancellationToken cancellationToken = default)
        {
            if (!_profile.IsOnline)
            {
                return false;
            }
            if (!grains.Any())
            {
                return true;
            }
            using (var conn = _profile.Connection)
            {
                await conn.OpenAsync(cancellationToken);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"{GrainAccessConfig<TDialect>.SQLAclCheck}";
                    _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, (await GetContextPrimaryRoleAsync(cancellationToken)).Id, desiredAccess);

                    var idsToCheck = new HashSet<Guid>();
                    var i = 0;
                    var grainsClause = grains.Aggregate(string.Empty, (aggr, grain) =>
                    {
                        if (null == grain)
                        {
                            if (_logger.IsEnabled(LogLevel.Warning))
                            {
                                _logger.LogWarning("Null grain supplied, skipping");
                            }
                            return aggr;
                        }
                        idsToCheck.Add(grain.Id);
                        var result = aggr;
                        if (0 < result.Length)
                        {
                            result += " OR ";
                        }
                        var param = $"{GeneralEntityDefaults.ParamId}{i++}";
                        result += $"x.{GeneralEntityDefaults.FieldId} = @{param}";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(param, grain.Id));
                        return result;
                    });
                    if (0 < idsToCheck.Count)
                    {
                        if (0 < grainsClause.Length)
                        {
                            cmd.CommandText += $"({grainsClause})";
                        }
                        using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                        {
                            if (!rs.HasRows)
                            {
                                return false;
                            }
                            while (await rs.ReadAsync(cancellationToken))
                            {
                                idsToCheck.Remove(rs.GetGuid(0));
                            }
                        }
                    }
                    return 0 == idsToCheck.Count;
                }
            }
        }

        public bool VerifyRoleEntitlement(RoleEntitlement roleCapability, bool includeAllRoles = false)
        {
            return VerifyRoleEntitlementAsync(roleCapability, includeAllRoles).Result;
        }

        public async Task<bool> VerifyRoleEntitlementAsync(RoleEntitlement roleCapability, bool includeAllRoles = false, CancellationToken cancellationToken = default)
        {
            var roles = await GetContextRolesAsync(cancellationToken);
            return includeAllRoles
                ? roles.Any(x => (roleCapability & x.Entitlement) == roleCapability)
                : (roleCapability & roles.FirstOrDefault()?.Entitlement) == roleCapability;
        }
    }
}
