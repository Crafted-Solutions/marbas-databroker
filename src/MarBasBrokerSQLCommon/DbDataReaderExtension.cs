using System.Data.Common;

namespace CraftedSolutions.MarBasBrokerSQLCommon
{
    public static class DbDataReaderExtension
    {

        public static Guid GetGuid(this DbDataReader dataReader, string fieldName)
        {
            return dataReader.GetGuid(dataReader.GetOrdinal(fieldName));
        }

        public static Guid? GetNullableGuid(this DbDataReader dataReader, string fieldName)
        {
            var ord = dataReader.GetOrdinal(fieldName);
            return dataReader.IsDBNull(ord) ? null : dataReader.GetGuid(ord);
        }

        public static DateTime GetDateTime(this DbDataReader dataReader, string fieldName)
        {
            var ord = dataReader.GetOrdinal(fieldName);
            var result = dataReader.IsDBNull(ord) ? DateTime.Now : dataReader.GetDateTime(ord);
            return DateTimeKind.Unspecified == result.Kind ? DateTime.SpecifyKind(result, DateTimeKind.Utc) : result.ToUniversalTime();
        }

        public static T? GetNullableField<T>(this DbDataReader dataReader, string fieldName, T? defaultVal = default)
        {
            var ord = dataReader.GetOrdinal(fieldName);
            return dataReader.IsDBNull(ord) ? defaultVal : dataReader.GetFieldValue<T>(ord);
        }
    }
}
