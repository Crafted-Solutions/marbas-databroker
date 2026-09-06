//#if DEBUG
//#define DEBUG_UPSERT
//#endif

using CraftedSolutions.MarBasBrokerSQLCommon.Access;
using CraftedSolutions.MarBasCommon.Reflection;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Broker;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace CraftedSolutions.MarBasBrokerSQLCommon
{
    using IConflictCollection = IEnumerable<(IEnumerable<string> Confilcts, IEnumerable<string>? Resolutions)>;

    public abstract class AbstractDbParameterFactory<TFactory> : IDbParameterFactory
        where TFactory : IDbParameterFactory, new()
    {
        public static readonly TFactory Instance = new();

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

        public virtual string PrepareObjectInserParameters<TFieldIFace, TAdapter>(DbParameterCollection parameters, TFieldIFace? valueProvider = null, IDictionary<string, (Type, object?)>? additionalValues = null)
            where TAdapter : AbstractDataAdapter
            where TFieldIFace : class
        {
            var (Colums, Parameters) = ExtractParametersFrom<TFieldIFace, TAdapter>(parameters, valueProvider, additionalValues);
            return $"({string.Join(",", Colums)}) VALUES (@{string.Join(",@", Parameters)})";
        }

        public virtual string PrepareObjectUpdateParameters<TFieldIFace, TAdapter>(DbParameterCollection parameters, TFieldIFace? valueProvider = null, IDictionary<string, (Type, object?)>? additionalValues = null)
            where TAdapter : AbstractDataAdapter
            where TFieldIFace : class
        {
            if (null == valueProvider && null == additionalValues)
            {
                throw new ArgumentException($"{nameof(valueProvider)} and {nameof(additionalValues)} cannot both be null");
            }
            var result = new StringBuilder();
            if (null != valueProvider)
            {
                var providerProps = typeof(TFieldIFace).GetAllProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Where(x =>
                    true != ((ReadOnlyAttribute?)Attribute.GetCustomAttribute(x, typeof(ReadOnlyAttribute)))?.IsReadOnly);

                foreach (var prop in providerProps)
                {
                    var paramName = $"param{prop.Name}";
                    parameters.Add(Create(paramName, prop.PropertyType, prop.GetValue(valueProvider)));

                    if (0 < result.Length)
                    {
                        result.Append(',');
                    }
                    result.Append(AbstractDataAdapter.GetAdapterColumnName<TAdapter>(prop.Name));
                    result.Append("=@");
                    result.Append(paramName);
                }
            }
            if (null != additionalValues)
            {
                foreach (var addVal in additionalValues)
                {
                    var paramName = $"param{addVal.Key}";
                    parameters.Add(Create(paramName, addVal.Value.Item1, addVal.Value.Item2));

                    if (0 < result.Length)
                    {
                        result.Append(',');
                    }
                    result.Append(addVal.Key);
                    result.Append("=@");
                    result.Append(paramName);
                }
            }
            return result.ToString();
        }

        public virtual string PrepareUpsertStatement<TFieldIFace, TAdapter>(string dataSource, IConflictCollection conflictUpdates,
            DbParameterCollection parameters, TFieldIFace? valueProvider = null, IDictionary<string, (Type, object?)>? additionalValues = null)
            where TAdapter : AbstractDataAdapter
            where TFieldIFace : class
        {
            var (Colums, Parameters) = ExtractParametersFrom<TFieldIFace, TAdapter>(parameters, valueProvider, additionalValues);
            var targetCols = string.Join(",", Colums);

            var result = new StringBuilder($"MERGE INTO {dataSource} t USING (SELECT ");
            result
                .Append($"@{string.Join(",@", Parameters)})")
                .Append($" AS s({targetCols})");

            static string MakeMatch(string name)
            {
                return $"t.{name} = s.{name}";
            }

            var resBuffer = new StringBuilder();
            var first = true;
            var hasNullRes = false;
            foreach (var (Confilcts, Resolutions) in conflictUpdates)
            {
                var match = string.Join(" AND ", Confilcts.Select(x => MakeMatch(x)));

                result
                    .Append(first ? " ON (" : " OR (")
                    .Append(match)
                    .Append(')');

                resBuffer
                    .Append(" WHEN MATCHED AND ")
                    .Append(match)
                    .Append(" THEN UPDATE SET ");
                if (null == Resolutions)
                {
                    if (hasNullRes)
                    {
                        throw new ArgumentException($"{nameof(conflictUpdates)} can only have one item where {nameof(Resolutions)} is null");
                    }
                    resBuffer.Append(string.Join(", ", Colums.Select(x => $"{x} = s.{x}")));
                    hasNullRes = true;
                }
                else
                {
                    resBuffer.Append(string.Join(", ", Resolutions.Select(x => $"{x} = s.{x}")));
                }
                first = false;
            }
            result
                .Append(resBuffer)
                .Append(" WHEN NOT MATCHED THEN INSERT (")
                .Append(targetCols)
                .Append(") VALUES (s.")
                .Append(string.Join(", s.", Colums))
                .Append(')');

#if DEBUG_UPSERT
            Console.WriteLine(result.ToString());
            Console.WriteLine($"[{string.Join(",", parameters.Cast<DbParameter>().Select(x => $"'{x.ParameterName}'={x.Value}"))}]");
#endif
            return result.ToString();
        }

        public abstract DbParameter PrepareTraitValueParameter(string paramName, TraitValueType valueType, object? value);

        public virtual string PrepareTraitComparison(DbParameterCollection parameters, TraitValueType valueType, object? value = null, FieldCompareOperator compareOperator = FieldCompareOperator.Eq, string paramName = "value")
        {
            string result;
            if (null == value)
            {
                result = $" {(compareOperator.IsNegated() ? "IS NOT" : "IS")} NULL";
            }
            else
            {
                if (compareOperator.HasFlag(FieldCompareOperator.Contains)
                    || compareOperator.HasFlag(FieldCompareOperator.StartsWith) || compareOperator.HasFlag(FieldCompareOperator.EndsWith))
                {
                    result = " LIKE";
                    if (compareOperator.IsNegated())
                    {
                        result = $" NOT{result}";
                    }
                    var strVal = value.ToString();
                    if (compareOperator.HasFlag(FieldCompareOperator.Contains))
                    {
                        strVal = $"%{strVal}%";
                    }
                    else if (compareOperator.HasFlag(FieldCompareOperator.StartsWith))
                    {
                        strVal = $"{strVal}%";
                    }
                    else if (compareOperator.HasFlag(FieldCompareOperator.EndsWith))
                    {
                        strVal = $"%{strVal}";
                    }
                    value = strVal;
                }
                else if (compareOperator.HasFlag(FieldCompareOperator.Gt) || compareOperator.HasFlag(FieldCompareOperator.Lt))
                {
                    result = $" {(compareOperator.HasFlag(FieldCompareOperator.Gt) ? ">" : "<")}";
                    if (compareOperator.HasFlag(FieldCompareOperator.Eq))
                    {
                        result += "=";
                    }
                }
                else
                {
                    result = $" {(compareOperator.IsNegated() ? "<>" : "=")}";
                }
                result += $" @{paramName}";
                parameters.Add(PrepareTraitValueParameter(paramName, valueType, value));
            }
            return result;
        }

        protected (IEnumerable<string> Colums, IEnumerable<string> Parameters) ExtractParametersFrom<TFieldIFace, TAdapter>(DbParameterCollection parameters, TFieldIFace? valueProvider = null, IDictionary<string, (Type, object?)>? additionalValues = null)
            where TAdapter : AbstractDataAdapter
            where TFieldIFace : class
        {
            if (null == valueProvider && null == additionalValues)
            {
                throw new ArgumentException($"{nameof(valueProvider)} and {nameof(additionalValues)} cannot both be null");
            }
            (IEnumerable<string> Colums, IEnumerable<string> Parameters) result = ([], []);
            if (null != valueProvider)
            {
                var providerProps = typeof(TFieldIFace).GetAllProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Where(x =>
                    true != ((ReadOnlyAttribute?)Attribute.GetCustomAttribute(x, typeof(ReadOnlyAttribute)))?.IsReadOnly);

                result.Colums = providerProps.Select(x => AbstractDataAdapter.GetAdapterColumnName<TAdapter>(x.Name));
                result.Parameters = providerProps.Select(x =>
                {
                    var paramName = $"param{x.Name}";
                    parameters.Add(Create(paramName, x.PropertyType, x.GetValue(valueProvider)));
                    return paramName;
                });

            }
            if (null != additionalValues)
            {
                result.Colums = result.Colums.Concat(additionalValues.Select(x => x.Key));
                result.Parameters = result.Parameters.Concat(additionalValues.Select(x =>
                {
                    var paramName = $"param{x.Key}";
                    parameters.Add(Create(paramName, x.Value.Item1, x.Value.Item2));
                    return paramName;
                }));
            }
            return result;
        }
    }
}
