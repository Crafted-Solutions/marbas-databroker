using MarBasBrokerSQLCommon;
using MarBasBrokerSQLCommon.GrainTier;

namespace MarBasBrokerEngineSQLite
{
    public sealed class SQLiteDialect : ISQLDialect
    {
        public string SubsrringFunc => "substr";

        public string GuidGen => "(SELECT result FROM mb_uuid)";

        public string ComparableDate(string dateExpression) => $"strftime('%s', {dateExpression})";

        public string SignedToUnsigned(string numberExpression) => $"(CAST({numberExpression} AS bigint) & 0xffffffff)";

        public string NewBlobContent(string? sizeParam = null) => $"zeroblob({(sizeParam ?? $"@{GrainFileDefaults.ParamSize}")})";

        public bool BlobUpdateRequiresReset => true;

        public string ReturnFromInsert => " RETURNING *";

        public string ReturnExistingBlobID(string table, string? column = null, string? param = null) => " RETURNING rowid";

        public string ReturnNewBlobID(string table, string? column = null, string? param = null) => " RETURNING last_insert_rowid()";

        public string GuidGenPerRow(string rowDiscriminator) => $"(SELECT result FROM mb_uuid WHERE {rowDiscriminator})";
    }
}
