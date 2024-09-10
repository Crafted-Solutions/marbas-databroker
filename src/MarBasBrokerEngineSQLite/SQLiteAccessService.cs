using MarBasBrokerSQLCommon.Access;
using MarBasSchema.Broker;
using Microsoft.Extensions.Logging;

namespace MarBasBrokerEngineSQLite
{
    public sealed class SQLiteAccessService : SQLAccessService<SQLiteDialect>
    {
        public SQLiteAccessService(IBrokerContext context, IBrokerProfile profile, ILogger<SQLiteAccessService> logger) : base(context, profile, logger)
        {
        }
    }
}
