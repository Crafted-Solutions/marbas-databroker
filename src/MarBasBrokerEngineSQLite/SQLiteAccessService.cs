using CraftedSolutions.MarBasBrokerSQLCommon.Access;
using CraftedSolutions.MarBasSchema.Broker;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasBrokerEngineSQLite
{
    public sealed class SQLiteAccessService : SQLAccessService<SQLiteDialect>
    {
        public SQLiteAccessService(IBrokerContext context, IBrokerProfile profile, ILogger<SQLiteAccessService> logger) : base(context, profile, logger)
        {
        }
    }
}
