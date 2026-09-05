using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Broker;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Security.Claims;
using System.Security.Principal;

namespace CraftedSolutions.MarBasBrokerSQLCommon.BrokerImpl
{
    public abstract class BaseSchemaBroker<TDialect> : IProfileProvider where TDialect : ISQLDialect, new()
    {
        #region Variables
        protected static readonly Random RandomSeed = new();

        protected readonly ISQLBrokerProfile _profile;
        protected readonly ILogger _logger;
        protected readonly IBrokerContext _context;
        protected readonly IAsyncAccessService _accessService;
        #endregion

        #region Construction
        protected BaseSchemaBroker(IBrokerProfile profile, ILogger logger) : this(profile, new AnonymousContext(), null!, logger)
        {
        }

#pragma warning disable IDE0290 // Use primary constructor
        protected BaseSchemaBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger logger)
#pragma warning restore IDE0290 // Use primary constructor
        {
            _profile = (ISQLBrokerProfile)profile;
            _logger = logger;
            _context = context;
            _accessService = accessService;
        }
        #endregion

        public IBrokerProfile Profile => _profile;

        #region Helper Methods

        protected virtual async Task<T?> WrapInTransaction<T>(T? defaultResult, Func<DbTransaction, Task<T>> func, CancellationToken cancellationToken)
        {
            T? result = defaultResult;
            using (var conn = _profile.Connection)
            {
                await conn.OpenAsync(cancellationToken);
                using (var ta = await conn.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        result = await func(ta);
                    }
                    catch
                    {
                        await ta.RollbackAsync(cancellationToken);
                        throw;
                    }
                    await ta.CommitAsync(cancellationToken);
                }
            }
            return result;
        }

        protected virtual async Task<T> ExecuteOnConnection<T>(T defaultResult, Func<DbCommand, Task<T>> func, CancellationToken cancellationToken)
        {
            T? result = defaultResult;
            using (var conn = _profile.Connection)
            {
                await conn.OpenAsync(cancellationToken);
                result = await func(conn.CreateCommand());
            }
            return result;
        }

        protected async Task CheckProfile(CancellationToken cancellationToken)
        {
            if (!await _profile.IsOnlineAsync(cancellationToken))
            {
                throw new InvalidOperationException("Schema is offline");
            }
        }

        protected void CheckBuiltIns(IEnumerable<IIdentifiable> ids)
        {
            if (ids.Any((x) => SchemaDefaults.BuiltInIds.Contains(x.Id)))
            {
                throw new UnauthorizedAccessException("At least one of elements belongs to built-in system schema and cannot be deleted");
            }
        }

        protected static string PrepareListOrderByClause<TFieldEnum, TAdapter>(IEnumerable<IListSortOption<TFieldEnum>>? sortOptions, string? fieldPrefix = null)
            where TFieldEnum : struct, Enum
            where TAdapter : AbstractDataAdapter
        {
            var result = string.Empty;
            if (null != sortOptions && sortOptions.Any())
            {
                var usedFields = new HashSet<string>();
                var pfx = string.IsNullOrEmpty(fieldPrefix) ? string.Empty : $"{fieldPrefix}.";
                var orderby = sortOptions.Aggregate(string.Empty, (aggr, elm) =>
                {
                    var field = Enum.GetName(elm.Field);
                    if (string.IsNullOrEmpty(field) || usedFields.Contains(field))
                    {
                        return aggr;
                    }
                    if (0 < aggr.Length)
                    {
                        aggr += ", ";
                    }

                    usedFields.Add(field);
                    return $"{aggr}{pfx}{AbstractDataAdapter.GetAdapterColumnName<TAdapter>(field)} {(Enum.GetName(elm.Order) ?? "ASC").ToUpperInvariant()}";
                });
                if (0 < orderby.Length)
                {
                    result = $" ORDER BY {orderby}";
                }
            }
            return result;
        }
        #endregion

        protected class AnonymousContext : IBrokerContext
        {
            public IPrincipal User => SchemaDefaults.AnonymousUser;

            public IEnumerable<string> UserRoles => ((ClaimsPrincipal)User).FindAll(ClaimTypes.Role).Select(x => x.Value);
        }
    }
}
