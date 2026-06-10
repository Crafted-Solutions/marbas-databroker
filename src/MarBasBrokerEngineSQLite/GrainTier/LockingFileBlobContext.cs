using CraftedSolutions.MarBasBrokerSQLCommon;
using CraftedSolutions.MarBasBrokerSQLCommon.GrainTier;
using CraftedSolutions.MarBasCommon.Reflection;
using System.Data.Common;

namespace CraftedSolutions.MarBasBrokerEngineSQLite.GrainTier
{
    internal sealed class LockingFileBlobContext(IDbConnectionProvider connectionProvider, Guid fileId, string column)
        : GrainFileBlobContext<SQLiteDialect, SQLiteParameterFactory>(connectionProvider, fileId, column)
    {
        private readonly IAsyncSchemaLock _lock = connectionProvider.CastOrThrow<IAsyncSchemaLock>();

        public async override Task<T> ExecuteOnConnection<T>(Func<DbCommand, Task<T>> func, CancellationToken cancellationToken = default)
        {
            using (await _lock.ReaderLockAsync(cancellationToken))
            {
                return await base.ExecuteOnConnection(func, cancellationToken);
            }
        }
    }
}
