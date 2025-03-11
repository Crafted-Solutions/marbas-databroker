using System.Data.Common;
using System.Globalization;
using CraftedSolutions.MarBasBrokerSQLCommon;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;
using Microsoft.Data.Sqlite;

namespace CraftedSolutions.MarBasBrokerEngineSQLite
{
    public sealed class SQLiteParameterFactory : AbstractDbParameterFactory<SQLiteParameterFactory>
    {
        public override DbParameter Create(string name, Type type, object? value)
        {
            SqliteParameter result;
            Type? effectiveType = type ?? ((dynamic?)value)?.GetType();
            if (effectiveType?.IsGenericType ?? false && effectiveType?.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                effectiveType = Nullable.GetUnderlyingType(effectiveType)!;
            }
            if (typeof(Guid).IsAssignableFrom(effectiveType) || typeof(IIdentifiable).IsAssignableFrom(effectiveType))
            {
                result = new SqliteParameter(name, SqliteType.Text)
                {
                    Value = null == value ? null : ((Guid)(dynamic)value).ToString("D")
                };
            }
            else if (typeof(DateTime).IsAssignableFrom(effectiveType))
            {
                result = new SqliteParameter(name, SqliteType.Text)
                {
                    Value = null == value ? null : ((dynamic)value).ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)
                };
            }
            else if (typeof(CultureInfo).IsAssignableFrom(effectiveType))
            {
                result = new SqliteParameter(name, SqliteType.Text)
                {
                    Value = null == value ? null : ((dynamic)value).IetfLanguageTag
                };
            }
            else if (typeof(byte[]).IsAssignableFrom(effectiveType))
            {
                result = new SqliteParameter(name, SqliteType.Blob)
                {
                    Value = value
                };
            }
            else if (typeof(float).IsAssignableFrom(effectiveType) || typeof(double).IsAssignableFrom(effectiveType) || typeof(decimal).IsAssignableFrom(effectiveType))
            {
                result = new SqliteParameter(name, SqliteType.Real)
                {
                    Value = value
                };
            }
            else if (typeof(int).IsAssignableFrom(effectiveType) || typeof(long).IsAssignableFrom(effectiveType)
                || typeof(uint).IsAssignableFrom(effectiveType) || typeof(ulong).IsAssignableFrom(effectiveType)
                || typeof(Enum).IsAssignableFrom(effectiveType))
            {
                result = new SqliteParameter(name, SqliteType.Integer)
                {
                    Value = value
                };
            }
            else
            {
                result = new SqliteParameter(name, (dynamic?)value);
            }
            if (null == result.Value)
            {
                result.Value = DBNull.Value;
            }
            return result;
        }

        public override DbParameter Update(DbParameter parameter, object? value, Type? type = null)
        {
            var result = parameter;
            var effectiveType = type ?? parameter.Value?.GetType() ?? value?.GetType();
            if (null != effectiveType && effectiveType.IsGenericType && effectiveType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                effectiveType = Nullable.GetUnderlyingType(effectiveType)!;
            }
            if (typeof(Guid).IsAssignableFrom(effectiveType) || typeof(IIdentifiable).IsAssignableFrom(effectiveType))
            {
                result.Value = null == value ? null : ((Guid)(dynamic)value).ToString("D");
            }
            else if (typeof(DateTime).IsAssignableFrom(type))
            {
                result.Value = null == value ? null : ((dynamic)value).ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
            }
            else if (typeof(CultureInfo).IsAssignableFrom(effectiveType))
            {
                result.Value = null == value ? null : ((dynamic)value).IetfLanguageTag;
            }
            else
            {
                result.Value = value;
            }
            if (null == result.Value)
            {
                result.Value = DBNull.Value;
            }
            return result;
        }

        public override DbParameter PrepareTraitValueParameter(string paramName, TraitValueType valueType, object? value)
        {
            DbParameter result;
            switch (valueType)
            {
                case TraitValueType.Grain:
                case TraitValueType.File:
                    {
                        result = new SqliteParameter(paramName, SqliteType.Text)
                        {
                            Value = null == value ? null : ((Guid)(dynamic)value).ToString("D")
                        };
                        break;
                    }
                case TraitValueType.DateTime:
                    {
                        result = new SqliteParameter(paramName, SqliteType.Text)
                        {
                            Value = null == value ? null : ((dynamic)value).ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)
                        };
                        break;
                    }
                default:
                    result = new SqliteParameter(paramName, value);
                    break;
            }
            if (null == result.Value)
            {
                result.Value = DBNull.Value;
            }
            return result;
        }
    }
}
