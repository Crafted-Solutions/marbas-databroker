using CraftedSolutions.MarBasBrokerSQLCommon;
using CraftedSolutions.MarBasBrokerSQLCommon.Access;
using CraftedSolutions.MarBasBrokerSQLCommon.GrainDef;

namespace CraftedSolutions.MarBasBrokerSQLCommon.Grain
{
    public class TraitBaseConfig<TDialect> where TDialect : ISQLDialect, new()
    {
        protected TraitBaseConfig() { }

        public const string SQLSelect = $"SELECT * FROM {TraitBaseDefaults.DataSourceExt} WHERE ";
        public static readonly string SQLSelectMeta =
$@"SELECT * FROM (
    SELECT *,
    RANK() OVER (
        PARTITION BY {GeneralEntityDefaults.FieldGrainId}, {TraitBaseDefaults.FieldPropDefId}, {TraitBaseDefaults.FieldOrd}
        ORDER BY {GeneralEntityDefaults.FieldLangCode} = @{GeneralEntityDefaults.ParamLang} DESC, {GeneralEntityDefaults.FieldLangCode} LIKE @{GeneralEntityDefaults.ParamLangShortLike} DESC, {EngineSpec<TDialect>.Dialect.SubsrringFunc}({GeneralEntityDefaults.FieldLangCode}, 0, 2), length({GeneralEntityDefaults.FieldLangCode}) DESC
    ) AS lang_rank
    FROM {TraitBaseDefaults.DataSourceExt}
    WHERE ({GeneralEntityDefaults.FieldLangCode} = @{GeneralEntityDefaults.ParamLangShort} OR {GeneralEntityDefaults.FieldLangCode} LIKE @{GeneralEntityDefaults.ParamLangPrefix} OR {GeneralEntityDefaults.FieldLangCode} = @{GeneralEntityDefaults.ParamLangDefault} OR {GeneralEntityDefaults.FieldLangCode} IS NULL)
)
WHERE lang_rank = 1";

        public static readonly string SQLSelectMetaWithDefaults =
$@"SELECT * FROM (
    SELECT *,
    RANK() OVER (
        PARTITION BY ({TraitBaseDefaults.FieldPropDefId})
        ORDER BY {TraitBaseDefaults.FieldPropDefId}, distance ASC
    ) AS trait_rank
    FROM (
        SELECT d.distance, t.* FROM (
            SELECT a.distance, g.{GeneralEntityDefaults.FieldId}
            FROM
            (
                SELECT {GrainTypeDefDefaults.MixinExtFieldBaseType}, distance, {GrainTypeDefDefaults.MixinExtFieldStart} FROM {GrainTypeDefDefaults.DataSourceTypeDefMixinAnc}
                UNION ALL
                SELECT
                (SELECT {GrainBaseConfig.FieldTypeDefId} FROM {GrainBaseConfig.DataSource} WHERE {GeneralEntityDefaults.FieldId} = @{GeneralEntityDefaults.ParamGrainId}) AS {GrainTypeDefDefaults.MixinExtFieldBaseType},
                -1 AS distance,
                (SELECT {GrainBaseConfig.FieldTypeDefId} FROM {GrainBaseConfig.DataSource} WHERE {GeneralEntityDefaults.FieldId} = @{GeneralEntityDefaults.ParamGrainId}) AS {GrainTypeDefDefaults.MixinExtFieldStart}
            )
            AS a
            JOIN {GrainBaseConfig.DataSource} AS g
            ON g.{GrainBaseConfig.FieldParentId} = a.{GrainTypeDefDefaults.MixinExtFieldBaseType} AND g.{GrainBaseConfig.FieldTypeDefId} = a.{GrainTypeDefDefaults.MixinExtFieldBaseType}
            WHERE a.{GrainTypeDefDefaults.MixinExtFieldStart} = (SELECT {GrainBaseConfig.FieldTypeDefId} FROM {GrainBaseConfig.DataSource} WHERE {GeneralEntityDefaults.FieldId} = @{GeneralEntityDefaults.ParamGrainId})
            UNION ALL
            SELECT -2 AS distance, @{GeneralEntityDefaults.ParamGrainId} AS {GeneralEntityDefaults.FieldId}
        ) AS d
        
        JOIN (
{SQLSelectMeta}
        ) AS t
        ON t.{GeneralEntityDefaults.FieldGrainId} = d.{GeneralEntityDefaults.FieldId}
        
        ORDER BY {TraitBaseDefaults.FieldPropDefId}, distance ASC, {TraitBaseDefaults.FieldOrd} ASC
    )
)
WHERE trait_rank = 1";

        public static readonly string SQLFilterByAcl =
@$"SELECT _t.{GeneralEntityDefaults.FieldId} FROM {TraitBaseDefaults.DataSource} AS _t JOIN (
SELECT g.{GeneralEntityDefaults.FieldId} FROM {GrainBaseConfig.DataSource} AS g
{GrainAccessConfig<TDialect>.SQLJoinAclCheck}
) AS aclcheck ON _t.{GeneralEntityDefaults.FieldGrainId} = aclcheck.{GeneralEntityDefaults.FieldId}
WHERE ";

        public const string SQLOrderByGrain = $"ORDER BY {GeneralEntityDefaults.FieldGrainId}, {GeneralEntityDefaults.FieldRevision}, {TraitBaseDefaults.FieldPropDefId}, {TraitBaseDefaults.FieldOrd}";
        public const string SQLWhereByGrain = $"{GeneralEntityDefaults.FieldGrainId} = @{GeneralEntityDefaults.ParamGrainId}";
        public static readonly string SQLSelectByGrain = $"{SQLSelectMeta} AND {SQLWhereByGrain} {SQLOrderByGrain}";
        public static readonly string SQLSelectWithDefaultsByGrain = $"{SQLSelectMetaWithDefaults} {SQLOrderByGrain}";
        public static readonly string SQLWhereByPropDef = $"{SQLWhereByGrain} AND {TraitBaseDefaults.FieldPropDefId} = @{TraitBaseDefaults.ParamPropDefId}";
        public static readonly string SQLSelectByPropDef = $"{SQLSelectMeta} AND {SQLWhereByPropDef} {SQLOrderByGrain}";

        public static readonly string SQLSelectWithDefaultsByPropDef = $"{SQLSelectMetaWithDefaults} AND {TraitBaseDefaults.FieldPropDefId} = @{TraitBaseDefaults.ParamPropDefId} ";

        public const string SQLDelete = $"DELETE FROM {TraitBaseDefaults.DataSource} AS t WHERE ";
        public const string SQLUpdate = $"UPDATE {TraitBaseDefaults.DataSource} SET ";
        public const string SQLInsert = $"INSERT INTO {TraitBaseDefaults.DataSource} ";

        public const string SQLUpdateTraitReindex =
$@"WITH trait_seq (seq, {GeneralEntityDefaults.FieldId}) AS (
    SELECT (ROW_NUMBER() OVER(
        PARTITION BY {GeneralEntityDefaults.FieldGrainId}, {GeneralEntityDefaults.FieldLangCode}, {GeneralEntityDefaults.FieldRevision}, {TraitBaseDefaults.FieldPropDefId}
        ORDER BY {GeneralEntityDefaults.FieldGrainId}, {GeneralEntityDefaults.FieldLangCode}, {GeneralEntityDefaults.FieldRevision}, {TraitBaseDefaults.FieldPropDefId}, {TraitBaseDefaults.FieldOrd}
    ) - 1) AS seq, {GeneralEntityDefaults.FieldId}
    FROM {TraitBaseDefaults.DataSource}
)
UPDATE {TraitBaseDefaults.DataSource}
SET {TraitBaseDefaults.FieldOrd} = (SELECT seq FROM trait_seq WHERE trait_seq.{GeneralEntityDefaults.FieldId} = {TraitBaseDefaults.DataSource}.{GeneralEntityDefaults.FieldId})";
    }
}
