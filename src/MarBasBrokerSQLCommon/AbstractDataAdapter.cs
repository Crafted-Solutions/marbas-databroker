using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;
using MarBasSchema;

namespace MarBasBrokerSQLCommon
{
    public abstract class AbstractDataAdapter: IUpdateable
    {
        public static string GetAdapterColumnName<TAdapter>(string fieldName) where TAdapter : AbstractDataAdapter
        {
            return GetMappedColumnNameByPropInfo(typeof(TAdapter).GetProperty(fieldName));
        }

        protected readonly DbDataReader _dataReader;

        protected AbstractDataAdapter(DbDataReader dataReader)
        {
            _dataReader = dataReader;
            if (!_dataReader.HasRows)
            {
                throw new ArgumentException("Data is empty");
            }
        }

        protected Guid? GetNullableGuid(string fieldName)
        {
            var ord = _dataReader.GetOrdinal(fieldName);
            return _dataReader.IsDBNull(ord) ? null : _dataReader.GetGuid(ord);
        }

        protected Guid GetGuid(string fieldName)
        {
            return _dataReader.GetGuid(_dataReader.GetOrdinal(fieldName));
        }

        protected DateTime GetDateTime(string fieldName)
        {
            var ord = _dataReader.GetOrdinal(fieldName);
            var result = _dataReader.IsDBNull(ord) ? DateTime.Now : _dataReader.GetDateTime(ord);
            return DateTimeKind.Unspecified == result.Kind ? DateTime.SpecifyKind(result, DateTimeKind.Utc) : result.ToUniversalTime();
        }

        protected T? GetNullableField<T>(string fieldName, T? defaultVal = default)
        {
            var ord = _dataReader.GetOrdinal(fieldName);
            return _dataReader.IsDBNull(ord) ? defaultVal : _dataReader.GetFieldValue<T>(ord);
        }

        protected string GetMappedColumnName([CallerMemberName] string? name = null)
        {
            return GetMappedColumnNameByPropInfo(GetType().GetProperty(name!, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
        }

        public static string GetMappedColumnNameByPropInfo(PropertyInfo? prop)
        {
            string? result = null;
            if (null != prop)
            {
                result = ((ColumnAttribute?)Attribute.GetCustomAttribute(prop, typeof(ColumnAttribute)))?.Name;
            }
            return (result ?? (prop?.Name.ToLowerInvariant()))!;
        }

        public ISet<string> GetDirtyFields<TScope>() => System.Collections.Immutable.ImmutableHashSet<string>.Empty;

        public UpdateableTracker FieldTracker => null!;


        public interface IColumnMapper
        {
            string? GetColumnName(string fieldName, IUpdateable updateable);
        }

        public interface IFieldValueMapper
        {
            Type? GetFieldType(string fieldName);
            object? MapFieldValue(string fieldName, object? origValue);
        }
    }
}
