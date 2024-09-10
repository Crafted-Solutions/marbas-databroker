using MarBasBrokerEngineSQLite.Resources;
using MarBasBrokerSQLCommon;
using MarBasBrokerSQLCommon.Grain;
using MarBasBrokerSQLCommon.GrainTier;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MarBasBrokerEngineSQLite
{
    public sealed class SQLiteProfile : SQLBrokerProfile<SqliteConnection, SqliteConnectionStringBuilder>
    {
        public static readonly Version SchemaVersion = new (0, 1, 12);

        public SQLiteProfile(IConfiguration configuration, ILogger<SQLiteProfile> logger)
            : base(configuration, logger)
        {
        }

        public override Version Version => SchemaVersion;

        public override IDbParameterFactory ParameterFactory => SQLiteParameterFactory.Instance;

        protected override SqliteConnectionStringBuilder ConnectionSettings
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionSettings.DataSource))
                {
                    _connectionSettings.DataSource = _configuration.GetValue("BrokerProfile:DataSource", "Data/marbas.sqlite")!;
                    //_connectionSettings.Version = _configuration.GetValue("BrokerProfile:Version", 3);
                    _connectionSettings.Pooling = _configuration.GetValue("BrokerProfile:Pooling", true);
                }
                return _connectionSettings;
            }
        }

        protected async override Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            return File.Exists(ConnectionSettings.DataSource) || await InitializeDBAsync(cancellationToken);
        }

        private async Task<bool> InitializeDBAsync(CancellationToken cancellationToken)
        {
            try
            { 
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Initializing profile {dataSource}", ConnectionSettings.DataSource);
                }
                using (var conn = Connection)
                {
                    await conn.OpenAsync(cancellationToken);
                    using (var ta = await conn.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken))
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = BootstrapSQL.Scheme;
                            cmd.Transaction = (SqliteTransaction?)ta;
                            await cmd.ExecuteNonQueryAsync(cancellationToken);
                        }
                        await ta.CommitAsync(cancellationToken);
                    }

                    using (var ta = await conn.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken))
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = BootstrapSQL.BasicData;
                            cmd.Transaction = (SqliteTransaction?)ta;
                            await cmd.ExecuteNonQueryAsync(cancellationToken);
                        }
                        await ta.CommitAsync(cancellationToken);
                    }

                    using (var ta = await conn.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken))
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = $"{GrainFileConfig<SQLiteDialect>.SQLInsertFile}({GeneralEntityDefaults.FieldBaseId}, {GrainFileDefaults.FieldMimeType}, {GrainFileDefaults.FieldSize}, {GrainFileDefaults.FieldContent}) VALUES ((SELECT {GeneralEntityDefaults.FieldId} FROM {GrainBaseConfig.DataSource} WHERE name = 'marbas.png'), 'image/png', @{GrainFileDefaults.ParamSize}, @{GrainFileDefaults.ParamContent})";
                            cmd.Parameters.Add(GrainFileDefaults.ParamSize, SqliteType.Integer).Value = BootstrapSQL.marbasPng.Length;
                            cmd.Parameters.Add(GrainFileDefaults.ParamContent, SqliteType.Blob, BootstrapSQL.marbasPng.Length).Value = BootstrapSQL.marbasPng;
                            await cmd.ExecuteNonQueryAsync(cancellationToken);
                        }
                        await ta.CommitAsync(cancellationToken);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Bootstrap error");
                return false;
            }
            return true;
        }
    }
}