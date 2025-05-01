using CraftedSolutions.MarBasBrokerSQLCommon.Access;

namespace CraftedSolutions.MarBasBrokerSQLCommon.Grain
{
    public class GrainLocalizedConfig<TDialect> where TDialect : ISQLDialect, new()
    {
        protected GrainLocalizedConfig() { }

        public static readonly string SQLSelectLabel = $"SELECT * FROM {GrainLocalizedDefaults.DataSourceLabel} WHERE ";

        public static readonly string SQLJoinLabel =
$@"LEFT JOIN (
        SELECT * FROM (
            SELECT *,
            RANK() OVER(
                PARTITION BY {GeneralEntityDefaults.FieldGrainId}
                ORDER BY {GeneralEntityDefaults.FieldLangCode} = @{GeneralEntityDefaults.ParamLang} DESC, {GeneralEntityDefaults.FieldLangCode} LIKE @{GeneralEntityDefaults.ParamLangShortLike} DESC, {EngineSpec<TDialect>.Dialect.SubsrringFunc}({GeneralEntityDefaults.FieldLangCode}, 0, 2), length({GeneralEntityDefaults.FieldLangCode}) DESC
            ) AS lang_rank
            FROM {GrainLocalizedDefaults.DataSourceLabel}
        )
        WHERE ({GeneralEntityDefaults.FieldLangCode} = @{GeneralEntityDefaults.ParamLangShort} OR {GeneralEntityDefaults.FieldLangCode} LIKE @{GeneralEntityDefaults.ParamLangPrefix} OR {GeneralEntityDefaults.FieldLangCode} = @{GeneralEntityDefaults.ParamLangDefault})
        AND lang_rank = 1
    ) AS l
    ON l.{GeneralEntityDefaults.FieldGrainId} = g.{GeneralEntityDefaults.FieldId}";

        public static readonly string SQLSelectLocalized =
$@"SELECT g.*, l.{GrainLocalizedDefaults.FieldLabel}, l.{GeneralEntityDefaults.FieldLangCode}
    FROM {GrainBaseConfig.DataSourceExt} AS g
    {SQLJoinLabel} WHERE ";

        public static readonly string SQLSelectByAclLocalizedTrunk =
$@"SELECT g.*, l.{GrainLocalizedDefaults.FieldLabel}, l.{GeneralEntityDefaults.FieldLangCode}, x.{AclDefaults.FieldAccessMask} AS permissions
    FROM {GrainBaseConfig.DataSourceExt} AS g
    {GrainAccessConfig<TDialect>.SQLJoinAclCheck}
    {SQLJoinLabel} ";

        public static readonly string SQLSelectByAclLocalized = $"{SQLSelectByAclLocalizedTrunk}WHERE ";

        public const string SQLInsertLabel = $"INSERT INTO {GrainLocalizedDefaults.DataSourceLabel} ";
        public static readonly string SQLUpdateLabel = $"{SQLInsertLabel} ({GeneralEntityDefaults.FieldGrainId}, {GeneralEntityDefaults.FieldLangCode}, {GrainLocalizedDefaults.FieldLabel}) VALUES (@{GeneralEntityDefaults.ParamId}, @{GeneralEntityDefaults.ParamLangCode}, @{GrainLocalizedDefaults.ParamLabel}) ON CONFLICT({GeneralEntityDefaults.FieldGrainId}, {GeneralEntityDefaults.FieldLangCode}) DO UPDATE SET {GrainLocalizedDefaults.FieldLabel} = {EngineSpec<TDialect>.Dialect.ConflictExcluded(GrainLocalizedDefaults.FieldLabel)}";
        public const string SQLDeleteLabel = $"DELETE FROM {GrainLocalizedDefaults.DataSourceLabel} WHERE ";

        public static readonly string SQLSelectPathByAclLocalized =
$@"SELECT g.*, l.{GrainLocalizedDefaults.FieldLabel}, l.{GeneralEntityDefaults.FieldLangCode}, x.{AclDefaults.FieldAccessMask} AS permissions
    FROM {GrainBaseConfig.DataSourcePath} AS a
    LEFT JOIN {GrainBaseConfig.DataSourceExt} AS g ON g.{GeneralEntityDefaults.FieldId} = a.{GeneralEntityDefaults.FieldId}
    {GrainAccessConfig<TDialect>.SQLJoinAclCheck}
    {SQLJoinLabel} WHERE ";

    }
}
