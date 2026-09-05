using System.Data.Common;
using System.Globalization;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Broker;
using static CraftedSolutions.MarBasBrokerSQLCommon.AbstractDataAdapter;

namespace CraftedSolutions.MarBasBrokerSQLCommon
{
    using IConflictCollection = IEnumerable<(IEnumerable<string> Confilcts, IEnumerable<string>? Resolutions)>;

    public interface IDbParameterFactory
    {
        DbParameter Create<TVal>(string name, TVal? value);
        DbParameter Create(string name, Type type, object? value);
        DbParameter Update<TVal>(DbParameter parameter, TVal? value);
        DbParameter Update(DbParameter parameter, object? value, Type? type = null);


        void AddParametersForCultureLayer(DbParameterCollection parameters, CultureInfo? culture = null);
        void AddParametersForGrainAclCheck(DbParameterCollection parameters, Guid currentUserRole, GrainAccessFlag desiredAccess = GrainAccessFlag.Read);
        string PrepareDirtyFieldsUpdate<TFieldMapper, TScope>(DbParameterCollection parameters, IUpdateable updateable, IColumnMapper? mapper = null, IFieldValueMapper? valueMapper = null)
            where TFieldMapper : AbstractDataAdapter;
        string PrepareObjectInserParameters<TFieldIFace, TAdapter>(DbParameterCollection parameters, TFieldIFace? valueProvider = null, IDictionary<string, (Type, object?)>? additionalValues = null)
            where TAdapter : AbstractDataAdapter where TFieldIFace : class;
        string PrepareObjectUpdateParameters<TFieldIFace, TAdapter>(DbParameterCollection parameters, TFieldIFace? valueProvider = null, IDictionary<string, (Type, object?)>? additionalValues = null)
            where TAdapter : AbstractDataAdapter where TFieldIFace : class;
        string PrepareUpsertStatement<TFieldIFace, TAdapter>(string dataSource, IConflictCollection conflictUpdates,
            DbParameterCollection parameters, TFieldIFace? valueProvider = null, IDictionary<string, (Type, object?)>? additionalValues = null)
            where TAdapter : AbstractDataAdapter where TFieldIFace : class;
        DbParameter PrepareTraitValueParameter(string paramName, TraitValueType valueType, object? value);
        string PrepareTraitComparison(DbParameterCollection parameters, TraitValueType valueType, object? value = null, FieldCompareOperator compareOperator = FieldCompareOperator.Eq, string paramName = "value");
    }

    public interface IDbParameterFactoryProvider
    {
        IDbParameterFactory ParameterFactory { get; }
    }
}
