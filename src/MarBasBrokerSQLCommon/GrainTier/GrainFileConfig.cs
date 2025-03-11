using CraftedSolutions.MarBasBrokerSQLCommon.Access;
using CraftedSolutions.MarBasBrokerSQLCommon.Grain;

namespace CraftedSolutions.MarBasBrokerSQLCommon.GrainTier
{
    public class GrainFileConfig<TDialect> where TDialect : ISQLDialect, new()
    {
        protected GrainFileConfig() { }

        public static readonly string SQLSelectFileTier = $"SELECT f.{GeneralEntityDefaults.FieldBaseId} AS {GeneralEntityDefaults.FieldId}, f.{GrainFileDefaults.FieldMimeType}, f.{GrainFileDefaults.FieldSize}, f.{GrainFileDefaults.FieldContent} FROM {GrainFileDefaults.DataSourceFile} AS f WHERE ";
        public static readonly string SQLSelectFile =
@$"SELECT g.*, f.{GrainFileDefaults.FieldMimeType}, f.{GrainFileDefaults.FieldSize}, l.{GrainLocalizedDefaults.FieldLabel}, l.{GeneralEntityDefaults.FieldLangCode}
    FROM {GrainBaseConfig.DataSourceExt} g
    {GrainLocalizedConfig<TDialect>.SQLJoinLabel}
    LEFT JOIN {GrainFileDefaults.DataSourceFile} AS f
    ON f.{GeneralEntityDefaults.FieldBaseId} = g.{GeneralEntityDefaults.FieldId} WHERE ";
        public static readonly string SQLSelectFileByAcl =
@$"SELECT g.*, f.{GrainFileDefaults.FieldMimeType}, f.{GrainFileDefaults.FieldSize}, l.{GrainLocalizedDefaults.FieldLabel}, l.{GeneralEntityDefaults.FieldLangCode}, x.{AclDefaults.FieldAccessMask} AS permissions
    FROM {GrainBaseConfig.DataSourceExt} g
    {GrainAccessConfig<TDialect>.SQLJoinAclCheck}
    {GrainLocalizedConfig<TDialect>.SQLJoinLabel}
    LEFT JOIN {GrainFileDefaults.DataSourceFile} AS f
    ON f.{GeneralEntityDefaults.FieldBaseId} = g.{GeneralEntityDefaults.FieldId} WHERE ";
        public static readonly string SQLSelectFileWithContent =
@$"SELECT g.*, f.{GrainFileDefaults.FieldMimeType}, f.{GrainFileDefaults.FieldSize}, f.{GrainFileDefaults.FieldContent}, l.{GrainLocalizedDefaults.FieldLabel}, l.{GeneralEntityDefaults.FieldLangCode}
    FROM {GrainBaseConfig.DataSourceExt} g
    {GrainLocalizedConfig<TDialect>.SQLJoinLabel}
    LEFT JOIN {GrainFileDefaults.DataSourceFile} AS f
    ON f.{GeneralEntityDefaults.FieldBaseId} = g.{GeneralEntityDefaults.FieldId} WHERE ";
        public static readonly string SQLSelectFileByAclWithContent =
@$"SELECT g.*, f.{GrainFileDefaults.FieldMimeType}, f.{GrainFileDefaults.FieldSize}, f.{GrainFileDefaults.FieldContent}, l.{GrainLocalizedDefaults.FieldLabel}, l.{GeneralEntityDefaults.FieldLangCode}, x.{AclDefaults.FieldAccessMask} AS permissions
    FROM {GrainBaseConfig.DataSourceExt} g
    {GrainAccessConfig<TDialect>.SQLJoinAclCheck}
    {GrainLocalizedConfig<TDialect>.SQLJoinLabel}
    LEFT JOIN {GrainFileDefaults.DataSourceFile} AS f
    ON f.{GeneralEntityDefaults.FieldBaseId} = g.{GeneralEntityDefaults.FieldId} WHERE ";

        public const string SQLSelectFileContent = $"SELECT content FROM {GrainFileDefaults.DataSourceFile} WHERE ";

        public const string SQLInsertFile = $"INSERT INTO {GrainFileDefaults.DataSourceFile} ";
        public const string SQLUpdateFile = $"UPDATE {GrainFileDefaults.DataSourceFile} SET ";
        public const string SQLDeleteFile = $"DELETE FROM {GrainFileDefaults.DataSourceFile} WHERE ";

    }
}
