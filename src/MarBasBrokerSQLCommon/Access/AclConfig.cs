namespace MarBasBrokerSQLCommon.Access
{
    public class AclConfig<TDialect> where TDialect : ISQLDialect, new()
    {
        public const string SQLSelectAcl = $"SELECT * FROM {AclDefaults.DataSourceAcl} WHERE ";
        public const string SQLSelectAclEffective = $"SELECT * FROM {AclDefaults.DataSourceAclEffective} WHERE ";

        public const string SQLInsertAcl = $"INSERT INTO {AclDefaults.DataSourceAcl} ";
        public const string SQLDeleteAcl = $"DELETE FROM {AclDefaults.DataSourceAcl} WHERE ";
        public const string SQLUpdateAcl = $"UPDATE {AclDefaults.DataSourceAcl} SET ";
    }
}
