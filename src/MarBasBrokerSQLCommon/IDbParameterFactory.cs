using System.Data.Common;
using System.Globalization;
using MarBasSchema;
using MarBasSchema.Access;
using static MarBasBrokerSQLCommon.AbstractDataAdapter;

namespace MarBasBrokerSQLCommon
{
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
        DbParameter PrepareTraitValueParameter(string paramName, TraitValueType valueType, object? value);
    }

    public interface IDbParameterFactoryProvider
    {
        IDbParameterFactory ParameterFactory { get; }
    }
}
