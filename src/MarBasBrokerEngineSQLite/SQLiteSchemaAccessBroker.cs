using CraftedSolutions.MarBasBrokerSQLCommon.BrokerImpl;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Broker;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasBrokerEngineSQLite
{
    public sealed class SQLiteSchemaAccessBroker : AclManagementBroker<SQLiteDialect>, ISchemaAccessBroker, IAsyncSchemaAccessBroker
    {
        public SQLiteSchemaAccessBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger<SQLiteSchemaAccessBroker> logger)
            : base(profile, context, accessService, logger)
        {
        }
    }
}
