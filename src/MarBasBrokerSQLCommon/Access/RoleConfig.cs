using CraftedSolutions.MarBasBrokerSQLCommon;

namespace CraftedSolutions.MarBasBrokerSQLCommon.Access
{
    public class RoleConfig<TDialect> where TDialect : ISQLDialect, new()
    {
        public const string SQLSelect = $"SELECT * FROM {RoleDefaults.DataSourceRole}";
        public const string SQLSelectRole = $"{SQLSelect} WHERE ";
        public const string SQLInsertRole = $"INSERT INTO {RoleDefaults.DataSourceRole} ";
        public const string SQLDeleteRole = $"DELETE FROM {RoleDefaults.DataSourceRole} WHERE ";
        public const string SQLUpdateRole = $"UPDATE {RoleDefaults.DataSourceRole} SET ";
    }
}
