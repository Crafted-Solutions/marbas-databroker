namespace CraftedSolutions.MarBasBrokerSQLCommon.Sys
{
    public class SystemLanguageConfig<TDialect> where TDialect : ISQLDialect, new()
    {
        protected SystemLanguageConfig() { }

        public const string SQLSelectLang = $"SELECT * FROM {SystemLanguageDefaults.DataSourceLang} WHERE ";
        public const string SQLDeleteLang = $"DELETE FROM {SystemLanguageDefaults.DataSourceLang} WHERE ";
        public const string SQLInsertLang = $"INSERT INTO {SystemLanguageDefaults.DataSourceLang} ";
        public const string SQLUpdateLang = $"UPDATE {SystemLanguageDefaults.DataSourceLang} SET ";

    }
}
