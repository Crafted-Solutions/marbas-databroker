using System.Data;
using System.Data.Common;
using MarBasBrokerSQLCommon.Lob;

namespace MarBasBrokerSQLCommon.GrainTier
{
    public class GrainFileBlobContext<TDialect, TParamFactory>
        : IBlobContext
        where TDialect : ISQLDialect, new()
        where TParamFactory : IDbParameterFactory, new()
    {
        private bool _disposed;
        private readonly Guid _id;
        private readonly IDbConnectionProvider _provider;
        private DbConnection? _connection;
        private DbCommand? _command;

        public GrainFileBlobContext(IDbConnectionProvider connectionProvider, Guid fileId, string column)
        {
            _id = fileId;
            _provider = connectionProvider;
            DataColumn = column;
            _disposed = false;
        }

        public DbCommand Command => GetCommandAsync().Result;

        public async Task<DbCommand> GetCommandAsync(CancellationToken cancellationToken = default)
        {
            var conn = Connection;
            if (ConnectionState.Open != conn.State)
            {
                await conn.OpenAsync(cancellationToken);
            }
            _command?.Dispose();
            _command = conn.CreateCommand();
            _command.CommandText = $"{GrainFileConfig<TDialect>.SQLSelectFileContent}{GeneralEntityDefaults.FieldBaseId} = @{GeneralEntityDefaults.ParamId}";
            _command.Parameters.Add(AbstractDbParameterFactory<TParamFactory>.Instance.Create(GeneralEntityDefaults.ParamId, _id));
            return _command;
        }

        public DbConnection Connection
        {
            get
            {
                if (null == _connection)
                {
                    _connection = _provider.Connection;
                }
                return _connection;
            }
        }

        public string DataColumn { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Dispose();
                    _command?.Dispose();
                }
                _disposed = true;
            }

        }
    }
}
