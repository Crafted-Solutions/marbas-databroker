using CraftedSolutions.MarBasBrokerSQLCommon.Access;
using CraftedSolutions.MarBasSchema.Broker;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasBrokerEngineSQLite
{
    public sealed class SQLiteAccessService(IBrokerContext context, IBrokerProfile profile, ILogger<SQLiteAccessService> logger)
        : SQLAccessService<SQLiteDialect>(context, profile, logger)
    {
    }
}
