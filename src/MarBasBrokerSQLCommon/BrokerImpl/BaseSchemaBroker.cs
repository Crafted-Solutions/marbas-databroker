using System.ComponentModel;
using System.Data.Common;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using MarBasCommon;
using MarBasCommon.Reflection;
using MarBasSchema;
using MarBasSchema.Access;
using MarBasSchema.Broker;
using Microsoft.Extensions.Logging;

namespace MarBasBrokerSQLCommon.BrokerImpl
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

        protected BaseSchemaBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger logger)
        {
            _profile = (ISQLBrokerProfile)profile;
            _logger = logger;
            _context = context;
            _accessService = accessService;
        }
        #endregion

        public IBrokerProfile Profile => _profile;

        #region Helper Methods

        protected async Task<T?> WrapInTransaction<T>(T? defaultResult, Func<DbTransaction, Task<T>> func, CancellationToken cancellationToken)
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

        protected async Task<T> ExecuteOnConnection<T>(T defaultResult, Func<DbCommand, Task<T>> func, CancellationToken cancellationToken)
        {
            T? result = defaultResult;
            using (var conn = _profile.Connection)
            {
                await conn.OpenAsync(cancellationToken);
                result = await func(conn.CreateCommand());
            }
            return result;
        }

        protected void CheckProfile()
        {
            if (!_profile.IsOnline)
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

        protected string PrepareObjectInserParameters<TFieldIFace, TAdapter>(DbParameterCollection parameters, TFieldIFace valueProvider, IDictionary<string, (Type, object?)>? additionalValues = null)
            where TAdapter : AbstractDataAdapter
        {
            var providerProps = typeof(TFieldIFace).GetAllProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Where(x =>
                true != ((ReadOnlyAttribute?)Attribute.GetCustomAttribute(x, typeof(ReadOnlyAttribute)))?.IsReadOnly);

            var cols = providerProps.Select(x => AbstractDataAdapter.GetAdapterColumnName<TAdapter>(x.Name));
            var vals = providerProps.Select(x =>
            {
                var paramName = $"param{x.Name}";
                parameters.Add(_profile.ParameterFactory.Create(paramName, x.PropertyType, x.GetValue(valueProvider)));
                return paramName;
            });
            if (null != additionalValues)
            {
                cols = Enumerable.Concat(cols, additionalValues.Select(x => x.Key));
                vals = Enumerable.Concat(vals, additionalValues.Select(x =>
                {
                    var paramName = $"param{x.Key}";
                    parameters.Add(_profile.ParameterFactory.Create(paramName, x.Value.Item1, x.Value.Item2));
                    return paramName;
                }));
            }
            return $"({string.Join(",", cols)}) VALUES (@{string.Join(",@", vals)})";
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
                    var field = Enum.GetName<TFieldEnum>(elm.Field);
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

            public IEnumerable<string> UserRoles => ((ClaimsPrincipal) User).FindAll(ClaimTypes.Role).Select(x => x.Value);
        }
    }
}
