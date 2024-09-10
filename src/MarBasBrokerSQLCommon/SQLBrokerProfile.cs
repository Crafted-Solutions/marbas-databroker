using System.Data.Common;
using MarBasBrokerSQLCommon.Access;
using MarBasBrokerSQLCommon.Grain;
using MarBasCommon;
using MarBasCommon.DependencyInjection;
using MarBasSchema;
using MarBasSchema.Access;
using MarBasSchema.Broker;
using MarBasSchema.Event;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MarBasBrokerSQLCommon
{
    public interface ISQLBrokerProfile: IBrokerProfile, IDbConnectionProvider, IDbParameterFactoryProvider
    {
    }

    public abstract class SQLBrokerProfile<TConn, TConnSettings>
        : ISQLBrokerProfile, IDbConnectionProvider<TConn>, IAsyncInitService
        where TConn : DbConnection, new()
        where TConnSettings: DbConnectionStringBuilder, new()
    {
        protected readonly IConfiguration _configuration;
        protected readonly ILogger _logger;
        protected readonly IList<ISchemaRole> _rolesCache;
        protected readonly TConnSettings _connectionSettings = new();
        protected readonly object _locker = new();
        protected readonly SemaphoreSlim _semaphore = new(1, 1);

        protected bool _isOnline;

        public event EventHandler<SchemaModifiedEventArgs<IIdentifiable>>? SchemaModified;

        protected SQLBrokerProfile(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration ?? new ConfigurationBuilder().Build();
            _logger = logger;
            if (null == configuration)
            {
                _logger.LogWarning("Missing configuration, using defaults");
            }
            _rolesCache = new List<ISchemaRole>();
            SchemaModified += OnSchemaModfied;
        }

        public virtual bool IsOnline => _isOnline;

        public abstract Version Version { get; }

        public Guid InstanceId { get; protected set; } 

        public IEnumerable<ISchemaRole> SchemaRoles
        {
            get
            {
                try
                {
                    _semaphore.Wait();
                    return _rolesCache;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        public abstract IDbParameterFactory ParameterFactory { get; }

        public TConn Connection => (TConn)Activator.CreateInstance(typeof(TConn), ConnectionString)!;
        DbConnection IDbConnectionProvider.Connection => Connection;

        protected virtual string ConnectionString => ConnectionSettings.ToString();

        protected virtual TConnSettings ConnectionSettings => _connectionSettings;

        public async Task<bool> InitServiceAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = false;
                if (await CanConnectAsync(cancellationToken))
                {
                    using (var conn = Connection)
                    {
                        await conn.OpenAsync(cancellationToken);
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "SELECT val FROM mb_schema_opts WHERE name = 'schema.version'";
                            var val = await cmd.ExecuteScalarAsync(cancellationToken);
                            if (Version.TryParse(val?.ToString(), out Version? ver))
                            {
                                result = ver == Version;
                            }
                            if (!result && _logger.IsEnabled(LogLevel.Warning))
                            {
                                _logger.LogWarning("Incompatible schema version {ver}", ver);
                            }
                        }
                        if (result)
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "SELECT val FROM mb_schema_opts WHERE name = 'instace.id'";
                                var val = await cmd.ExecuteScalarAsync(cancellationToken);
                                if (Guid.TryParse(val?.ToString(), out Guid id))
                                {
                                    InstanceId = id;
                                }
                            }

                        }
                        if (result)
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = $"SELECT {GeneralEntityDefaults.FieldId} FROM {GrainBaseConfig.DataSource} WHERE name = @{GrainBaseConfig.ParamName}";
                                cmd.Parameters.Add(ParameterFactory.Create(GrainBaseConfig.ParamName, SchemaDefaults.RootName));
                                using (var r = await cmd.ExecuteReaderAsync(cancellationToken))
                                {
                                    result = r.Read() && SchemaDefaults.RootID.Equals(r.GetGuid(0));
                                    if (_logger.IsEnabled(LogLevel.Information))
                                    {
                                        _logger.LogInformation("Profile {typeName} ({version}) initialized successfully: {result}", GetType().Name, Version, result);
                                    }
                                }
                            }
                        }
                        if (result)
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = $"SELECT * FROM {RoleDefaults.DataSourceRole}";
                                using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                                {
                                    while (await rs.ReadAsync(cancellationToken))
                                    {
                                        _rolesCache.Add(new SchemaRole(new RoleDataAdapter(rs)));
                                    }
                                    if (_logger.IsEnabled(LogLevel.Information))
                                    {
                                        _logger.LogInformation("Loaded {rolesCount} schema roles", _rolesCache.Count);
                                    }
                                }
                            }
                        }
                    }
                }
                return _isOnline = result;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void DispatchSchemaModified<TSubject>(SchemaModificationType changeType, IEnumerable<IIdentifiable> subjects)
        {
            SchemaModified?.Invoke(this, new SchemaModifiedEventArgs<IIdentifiable>(changeType, subjects, typeof(TSubject)));
        }

        protected void OnSchemaModfied(object? sender, SchemaModifiedEventArgs<IIdentifiable> args)
        {
            if (!typeof(ISchemaRole).IsAssignableFrom(args.ConcreteSubjectType))
            {
                return;
            }
            _semaphore.Wait();
            try
            {
                var existing = _rolesCache.IntersectBy(args.Subjects.Select(x => x.Id), (x) => x.Id).ToList();
                foreach (var role in existing)
                {
                    if (SchemaModificationType.Update == args.ChangeType)
                    {
                        var upd = args.Subjects.First(x => x.Id == role.Id);
                        if (upd is ISchemaRole updRole)
                        {
                            var roleType = role.GetType();
                            var dirtyFields = updRole.GetDirtyFields<ISchemaRole>();
                            foreach (var fieldName in dirtyFields)
                            {
                                roleType.GetProperty(fieldName)?.SetValue(role, updRole.GetType().GetProperty(fieldName)?.GetValue(updRole));
                            }
                            dirtyFields.Clear();
                            role.GetDirtyFields<ISchemaRole>().Clear();
                        }
                    }
                    else
                    {
                        _rolesCache.Remove(role);
                    }
                }
                if (SchemaModificationType.Create == args.ChangeType)
                {
                    foreach (var id in args.Subjects)
                    {
                        if (id is ISchemaRole role)
                        {
                            _rolesCache.Add(role);
                        }
                        else if (_logger.IsEnabled(LogLevel.Warning))
                        {
                            _logger.LogWarning("Unable to register {id} as role due to incompatible type {type}", id.Id.ToString("D"), id.GetType().Name);
                        }
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected abstract Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
    }
}
