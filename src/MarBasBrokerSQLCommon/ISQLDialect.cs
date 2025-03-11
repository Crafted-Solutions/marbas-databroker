namespace CraftedSolutions.MarBasBrokerSQLCommon
{
    public interface ISQLDialect
    {
        string SubsrringFunc { get; }
        string GuidGen { get; }
        string ComparableDate(string dateExpression);
        string SignedToUnsigned(string numberExpression);
        string ConflictExcluded(string fieldName);
        string NewBlobContent(string? sizeParam = null);
        bool BlobUpdateRequiresReset { get; }
        string ReturnFromInsert { get; }
        string ReturnNewBlobID(string table, string? column = null, string? param = null);
        string ReturnExistingBlobID(string table, string? column = null, string? param = null);
        string GuidGenPerRow(string rowDiscriminator);
    }
}
