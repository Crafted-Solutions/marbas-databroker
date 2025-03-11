using System.Data.Common;
using CraftedSolutions.MarBasBrokerSQLCommon;
using CraftedSolutions.MarBasBrokerSQLCommon.GrainTier;
using CraftedSolutions.MarBasBrokerSQLCommon.Lob;
using CraftedSolutions.MarBasSchema.GrainTier;
using CraftedSolutions.MarBasSchema.IO;

namespace CraftedSolutions.MarBasBrokerEngineSQLite.GrainTier
{
    internal sealed class GrainFileDataAdapter : GrainFileInlineDataAdapter
    {
        private readonly IDbConnectionProvider? _connectionProvider;

        public GrainFileDataAdapter(DbDataReader dataReader, IDbConnectionProvider? connectionProvider = null)
            : base(dataReader, null == connectionProvider ? GrainFileContentAccess.None : GrainFileContentAccess.OnDemand)
        {
            _connectionProvider = connectionProvider;
        }

        public override IStreamableContent? Content
        {
            get
            {
                if (null == _connectionProvider)
                {
                    return base.Content;
                }
                return new SimpleStreamableBlob(new GrainFileBlobContext<SQLiteDialect, SQLiteParameterFactory>(_connectionProvider, Id, GetMappedColumnName()));
            }
            set => throw new NotImplementedException();
        }

    }
}
