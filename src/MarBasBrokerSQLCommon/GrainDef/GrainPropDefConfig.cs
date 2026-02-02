using CraftedSolutions.MarBasBrokerSQLCommon.Access;
using CraftedSolutions.MarBasBrokerSQLCommon.Grain;

namespace CraftedSolutions.MarBasBrokerSQLCommon.GrainDef
{
    public class GrainPropDefConfig<TDialect> where TDialect : ISQLDialect, new()
    {
        protected GrainPropDefConfig() { }

        public const string SQLSelectPropDefTier = $"SELECT g.{GeneralEntityDefaults.FieldBaseId} AS {GeneralEntityDefaults.FieldId}, g.* FROM {GrainPropDefDefaults.DataSourcePropDef} AS g WHERE ";
        public const string SQLSelectPropDef = $"SELECT g.* FROM {GrainPropDefDefaults.DataSourcePropDefExt} AS g WHERE ";

        public static readonly string SQLSelectPropDefLocalized =
$@"SELECT g.*, l.{GrainLocalizedDefaults.FieldLabel}, l.{GeneralEntityDefaults.FieldLangCode}
    FROM {GrainPropDefDefaults.DataSourcePropDefExt} AS g
    {GrainLocalizedConfig<TDialect>.SQLJoinLabel} WHERE ";

        public static readonly string SQLSelectPropDefByAclLocalizedNC =
$@"SELECT g.*, l.{GrainLocalizedDefaults.FieldLabel}, l.{GeneralEntityDefaults.FieldLangCode}, x.{AclDefaults.FieldAccessMask} AS permissions
    FROM {GrainPropDefDefaults.DataSourcePropDefExt} AS g
    {GrainAccessConfig<TDialect>.SQLJoinAclCheck}
    {GrainLocalizedConfig<TDialect>.SQLJoinLabel} ";
        public static readonly string SQLSelectPropDefByAclLocalized = $"{SQLSelectPropDefByAclLocalizedNC} WHERE ";

        public static readonly string SQLJoinByTypeDefWithInheritance =
@$"JOIN
(SELECT @{GrainTypeDefDefaults.ParamTypeDefId} AS type_id
    UNION ALL
    SELECT t.{GrainTypeDefDefaults.MixinExtFieldBaseType} AS type_id
    FROM {GrainTypeDefDefaults.DataSourceTypeDefMixinAnc} AS t
    WHERE t.{GrainTypeDefDefaults.MixinExtFieldStart} = @{GrainTypeDefDefaults.ParamTypeDefId}) AS m
ON g.{GrainBaseConfig.GrainExtFieldIdPath} LIKE '%' || m.type_id || '/%'";

        public const string SQLInsertPropDef = $"INSERT INTO {GrainPropDefDefaults.DataSourcePropDef} ";
        public const string SQLUpdatePropDef = $"UPDATE {GrainPropDefDefaults.DataSourcePropDef} SET ";
        public const string SQLDeletePropDef = $"DELETE FROM {GrainPropDefDefaults.DataSourcePropDef} WHERE ";
    }
}
