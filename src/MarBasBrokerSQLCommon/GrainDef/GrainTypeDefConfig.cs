using MarBasBrokerSQLCommon.Access;
using MarBasBrokerSQLCommon.Grain;

namespace MarBasBrokerSQLCommon.GrainDef
{
    public class GrainTypeDefConfig<TDialect> where TDialect : ISQLDialect, new()
    {
        protected GrainTypeDefConfig() { }

        public const string SQLSelectTypeDefTier = $"SELECT g.{GeneralEntityDefaults.FieldBaseId} AS {GeneralEntityDefaults.FieldId}, g.* FROM {GrainTypeDefDefaults.DataSourceTypeDef} g WHERE ";
        public const string SQLSelectTypeDef = $"SELECT g.* FROM {GrainTypeDefDefaults.DataSourceTypeDefExt} g WHERE ";

        public static readonly string SQLSelectTypeDefLocalized =
$@"SELECT g.*, l.{GrainLocalizedDefaults.FieldLabel}, l.{GeneralEntityDefaults.FieldLangCode}
    FROM {GrainTypeDefDefaults.DataSourceTypeDefExt} g
    {GrainLocalizedConfig<TDialect>.SQLJoinLabel} WHERE ";
        public static readonly string SQLSelectTypeDefByAclLocalized =
$@"SELECT g.*, l.{GrainLocalizedDefaults.FieldLabel}, l.{GeneralEntityDefaults.FieldLangCode}, x.{AclDefaults.FieldAccessMask} AS permissions
    FROM {GrainTypeDefDefaults.DataSourceTypeDefExt} g
    {GrainAccessConfig<TDialect>.SQLJoinAclCheck}
    {GrainLocalizedConfig<TDialect>.SQLJoinLabel} WHERE ";

        public const string SQLInsertTypeDef = $"INSERT INTO {GrainTypeDefDefaults.DataSourceTypeDef} ";
        public const string SQLUpdateTypeDef = $"UPDATE {GrainTypeDefDefaults.DataSourceTypeDef} SET ";
        public const string SQLDeleteTypeDef = $"DELETE FROM {GrainTypeDefDefaults.DataSourceTypeDef} WHERE ";

        public const string SQLSelectTypeDefMixinAnc = $"SELECT * FROM  {GrainTypeDefDefaults.DataSourceTypeDefMixinAnc} WHERE ";
        public const string SQLInsertTypeDefMixin = $"INSERT INTO {GrainTypeDefDefaults.DataSourceTypeDefMixin} ";
        public const string SQLDeleteTypeDefMixin = $"DELETE FROM {GrainTypeDefDefaults.DataSourceTypeDefMixin} WHERE ";

        public const string SQLSelectTypeDefMixinDefault = $"SELECT NULL AS {GrainTypeDefDefaults.MixinExtFieldDerivedType}, NULL AS {GrainTypeDefDefaults.MixinExtFieldBaseType}, 0 AS distance, ({GrainBaseConfig.SQLSelectTypeDef}{GeneralEntityDefaults.FieldId} = @{GeneralEntityDefaults.ParamId}) AS {GrainTypeDefDefaults.MixinExtFieldStart} UNION ALL ";
        public const string SQLSelectTypeDefMixinDescendants = $"SELECT m.{GrainTypeDefDefaults.MixinExtFieldDerivedType} FROM {GrainTypeDefDefaults.DataSourceTypeDefMixinDesc} AS m WHERE ";
    }
}
