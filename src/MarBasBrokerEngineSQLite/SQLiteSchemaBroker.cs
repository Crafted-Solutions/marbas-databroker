using CraftedSolutions.MarBasBrokerEngineSQLite.GrainTier;
using CraftedSolutions.MarBasBrokerSQLCommon;
using CraftedSolutions.MarBasBrokerSQLCommon.BrokerImpl;
using CraftedSolutions.MarBasBrokerSQLCommon.GrainTier;
using CraftedSolutions.MarBasCommon.Reflection;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Broker;
using CraftedSolutions.MarBasSchema.GrainTier;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace CraftedSolutions.MarBasBrokerEngineSQLite
{
    public sealed class SQLiteSchemaBroker : GrainTransportBroker<SQLiteDialect>, ISchemaBroker, IAsyncSchemaBroker
    {
        #region Variables
        private readonly IAsyncSchemaLock _lock;
        #endregion

        #region Construction
        public SQLiteSchemaBroker(IBrokerProfile profile, ILogger<SQLiteSchemaBroker> logger)
            : base(profile, logger)
        {
            _lock = profile.CastOrThrow<IAsyncSchemaLock>();
        }

        public SQLiteSchemaBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger<SQLiteSchemaBroker> logger)
            : base(profile, context, accessService, logger)
        {
            _lock = profile.CastOrThrow<IAsyncSchemaLock>();
        }
        #endregion

        #region Overrides
        protected override IGrainFile CreateFileAdapter(DbDataReader reader, GrainFileContentAccess loadContent = GrainFileContentAccess.OnDemand)
        {
            return new GrainFileDataAdapter(reader, GrainFileContentAccess.None == loadContent ? null : _profile);
        }

        protected override async Task WriteFileBlobAsync(DbConnection connection, Stream content, object blobId, CancellationToken cancellationToken = default)
        {
            using (var blob = new SqliteBlob((SqliteConnection)connection, GrainFileDefaults.DataSourceFile, MapFileColumn(nameof(IGrainFile.Content)), (long)blobId))
            {
                await content.CopyToAsync(blob, cancellationToken);
            }
        }

        protected override async Task CloneFileBlobInTA(Guid sourceFileId, Guid targetFileId, DbTransaction ta, CancellationToken cancellationToken)
        {
            long? srcBlobId = null;
            long? tgtBlobId = null;
            using (var cmd = ta.Connection!.CreateCommand())
            {
                cmd.CommandText = $"SELECT rowid FROM {GrainFileDefaults.DataSourceFile} WHERE {GeneralEntityDefaults.FieldBaseId} = @{GeneralEntityDefaults.ParamId}";
                var param = _profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, sourceFileId);
                cmd.Parameters.Add(param);

                srcBlobId = (long?)await cmd.ExecuteScalarAsync(cancellationToken);
                if (null == srcBlobId)
                {
                    throw new ApplicationException($"Failed to retrieve rowid for File {sourceFileId}");
                }

                _profile.ParameterFactory.Update(param, targetFileId);

                tgtBlobId = (long?)await cmd.ExecuteScalarAsync(cancellationToken);
                if (null == tgtBlobId)
                {
                    throw new ApplicationException($"Failed to retrieve rowid for File {targetFileId}");
                }
            }

            using (var srcBlob = new SqliteBlob((SqliteConnection)ta.Connection!, GrainFileDefaults.DataSourceFile, MapFileColumn(nameof(IGrainFile.Content)), (long)srcBlobId))
            using (var tgtBlob = new SqliteBlob((SqliteConnection)ta.Connection!, GrainFileDefaults.DataSourceFile, MapFileColumn(nameof(IGrainFile.Content)), (long)tgtBlobId))
            {
                await srcBlob.CopyToAsync(tgtBlob, cancellationToken);
            }

        }

        protected override async Task<T?> WrapInTransaction<T>(T? defaultResult, Func<DbTransaction, Task<T>> func, CancellationToken cancellationToken) where T : default
        {
            using (await _lock.WriterLockAsync(cancellationToken))
            {
                return await base.WrapInTransaction(defaultResult, func, cancellationToken);
            }
        }

        protected override async Task<T> ExecuteOnConnection<T>(T defaultResult, Func<DbCommand, Task<T>> func, CancellationToken cancellationToken)
        {
            using (await _lock.ReaderLockAsync(cancellationToken))
            {
                return await base.ExecuteOnConnection(defaultResult, func, cancellationToken);
            }
        }
        #endregion

    }
}
