using MarBasBrokerSQLCommon.BrokerImpl;
using MarBasSchema.Access;
using MarBasSchema.Broker;
using Microsoft.Extensions.Logging;

namespace MarBasBrokerEngineSQLite
{
    public sealed class SQLiteSchemaAccessBroker : AclManagementBroker<SQLiteDialect>, ISchemaAccessBroker, IAsyncSchemaAccessBroker
    {
        public SQLiteSchemaAccessBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger<SQLiteSchemaAccessBroker> logger)
            : base(profile, context, accessService, logger)
        {
        }
    }
}
