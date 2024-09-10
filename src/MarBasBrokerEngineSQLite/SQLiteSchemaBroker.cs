using System.Data.Common;
using MarBasBrokerEngineSQLite.GrainTier;
using MarBasBrokerSQLCommon;
using MarBasBrokerSQLCommon.BrokerImpl;
using MarBasBrokerSQLCommon.GrainTier;
using MarBasSchema.Access;
using MarBasSchema.Broker;
using MarBasSchema.GrainTier;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MarBasBrokerEngineSQLite
{
    public sealed class SQLiteSchemaBroker : GrainTransportBroker<SQLiteDialect>, ISchemaBroker, IAsyncSchemaBroker
    {

        #region Construction
        public SQLiteSchemaBroker(IBrokerProfile profile, ILogger<SQLiteSchemaBroker> logger) : base(profile, logger)
        {
        }

        public SQLiteSchemaBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger<SQLiteSchemaBroker> logger) : base(profile, context, accessService, logger)
        {
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
        #endregion

    }
}
