using System.Data.Common;
using System.Globalization;
using MarBasBrokerSQLCommon.Access;
using MarBasSchema;
using MarBasSchema.Access;

namespace MarBasBrokerSQLCommon
{
    public abstract class AbstractDbParameterFactory<TFactory> : IDbParameterFactory
        where TFactory: IDbParameterFactory, new()
    {
        public DbParameter Create<TVal>(string name, TVal? value)
        {
            return Create(name, typeof(TVal), value);
        }

        public abstract DbParameter Create(string name, Type type, object? value);

        public DbParameter Update<TVal>(DbParameter parameter, TVal? value)
        {
            return Update(parameter, value, typeof(TVal));
        }
        public abstract DbParameter Update(DbParameter parameter, object? value, Type? type = null);


        public virtual void AddParametersForCultureLayer(DbParameterCollection parameters, CultureInfo? culture = null)
        {
            var effCulture = culture ?? SchemaDefaults.Culture;
            parameters.Add(Create(GeneralEntityDefaults.ParamLang, effCulture.Name));
            parameters.Add(Create(GeneralEntityDefaults.ParamLangShortLike, $"{effCulture.TwoLetterISOLanguageName}%"));
            parameters.Add(Create(GeneralEntityDefaults.ParamLangShort, effCulture.TwoLetterISOLanguageName));
            parameters.Add(Create(GeneralEntityDefaults.ParamLangPrefix, $"{effCulture.TwoLetterISOLanguageName}-%"));
            parameters.Add(Create(GeneralEntityDefaults.ParamLangDefault, SchemaDefaults.Culture.Name));
        }

        public virtual void AddParametersForGrainAclCheck(DbParameterCollection parameters, Guid currentUserRole, GrainAccessFlag desiredAccess = GrainAccessFlag.Read)
        {
            parameters.Add(Create(GrainAccessDefaults.ParamCurrentRole, currentUserRole));
            parameters.Add(Create(GrainAccessDefaults.ParamEveryoneRole, SchemaDefaults.EveryoneRoleID));
            parameters.Add(Create(GrainAccessDefaults.ParamDesiredAccess, desiredAccess));
        }

        public virtual string PrepareDirtyFieldsUpdate<TFieldMapper, TScope>(DbParameterCollection parameters, IUpdateable updateable, AbstractDataAdapter.IColumnMapper? mapper = null, AbstractDataAdapter.IFieldValueMapper? valueMapper = null)
            where TFieldMapper : AbstractDataAdapter
        {
            var fieldsClause = "";
            var adapterType = typeof(TFieldMapper);
            foreach (var fieldName in updateable.GetDirtyFields<TScope>())
            {
                var prop = updateable.GetType().GetProperty(fieldName);
                if (null != prop)
                {
                    var paramName = $"p_{fieldName}";
                    var propVal = prop.GetValue(updateable);
                    if (null != valueMapper)
                    {
                        propVal = valueMapper.MapFieldValue(fieldName, propVal);
                    }
                    var propType = valueMapper?.GetFieldType(fieldName) ?? propVal?.GetType() ?? prop.GetType();
                    parameters.Add(Create(paramName, propType, propVal));
                    if (0 < fieldsClause.Length)
                    {
                        fieldsClause += ", ";
                    }
                    fieldsClause += $"{mapper?.GetColumnName(fieldName, updateable) ?? AbstractDataAdapter.GetMappedColumnNameByPropInfo(adapterType.GetProperty(fieldName))} = @{paramName}";
                }
            }
            return fieldsClause;
        }

        public abstract DbParameter PrepareTraitValueParameter(string paramName, TraitValueType valueType, object? value);

        public static readonly TFactory Instance = new ();
    }
}
