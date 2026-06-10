using System.Data.Common;
using CraftedSolutions.MarBasBrokerSQLCommon;
using CraftedSolutions.MarBasBrokerSQLCommon.GrainTier;
using CraftedSolutions.MarBasBrokerSQLCommon.Lob;
using CraftedSolutions.MarBasSchema.GrainTier;
using CraftedSolutions.MarBasSchema.IO;

namespace CraftedSolutions.MarBasBrokerEngineSQLite.GrainTier
{
    internal sealed class GrainFileDataAdapter(DbDataReader dataReader, IDbConnectionProvider? connectionProvider = null)
        : GrainFileInlineDataAdapter(dataReader, null == connectionProvider ? GrainFileContentAccess.None : GrainFileContentAccess.OnDemand)
    {
        private readonly IDbConnectionProvider? _connectionProvider = connectionProvider;

        public override IStreamableContent? Content
        {
            get
            {
                if (null == _connectionProvider)
                {
                    return base.Content;
                }
                return new SimpleStreamableBlob(new LockingFileBlobContext(_connectionProvider, Id, GetMappedColumnName()));
            }
            set => throw new NotImplementedException();
        }

    }
}
