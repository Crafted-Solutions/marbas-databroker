using CraftedSolutions.MarBasBrokerSQLCommon;
using CraftedSolutions.MarBasBrokerSQLCommon.Access;

namespace CraftedSolutions.MarBasBrokerSQLCommon.Grain
{
    public class GrainExtendedConfig<TDialect> where TDialect : ISQLDialect, new()
    {
        protected GrainExtendedConfig() { }

        public static readonly string SQLSelect = $"SELECT g.* FROM {GrainBaseConfig.DataSourceExt} AS g WHERE ";
        public static readonly string SQLSelectByAcl =
$@"SELECT g.*, x.{AclDefaults.FieldAccessMask} AS permissions
    FROM {GrainBaseConfig.DataSourceExt} AS g
    {GrainAccessConfig<TDialect>.SQLJoinAclCheck} WHERE ";
    }
}
