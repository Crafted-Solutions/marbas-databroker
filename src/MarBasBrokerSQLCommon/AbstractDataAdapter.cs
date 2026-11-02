using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;
using CraftedSolutions.MarBasSchema;

namespace CraftedSolutions.MarBasBrokerSQLCommon
{
    public abstract class AbstractDataAdapter : IUpdateable
    {
        protected static readonly UpdateableTracker DummyFieldTracker = new();

        protected readonly DbDataReader _dataReader;

        protected AbstractDataAdapter(DbDataReader dataReader)
        {
            _dataReader = dataReader;
            if (!_dataReader.HasRows)
            {
                throw new ArgumentException("Data is empty");
            }
        }

        protected Guid? GetNullableGuid(string fieldName) => _dataReader.GetNullableGuid(fieldName);

        protected Guid GetGuid(string fieldName) => _dataReader.GetGuid(fieldName);

        protected DateTime GetDateTime(string fieldName) => _dataReader.GetDateTime(fieldName);

        protected T? GetNullableField<T>(string fieldName, T? defaultVal = default) => _dataReader.GetNullableField<T>(fieldName, defaultVal);

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

        public UpdateableTracker FieldTracker => DummyFieldTracker;

        public static string GetAdapterColumnName<TAdapter>(string fieldName) where TAdapter : AbstractDataAdapter
        {
            return GetMappedColumnNameByPropInfo(typeof(TAdapter).GetProperty(fieldName));
        }

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
