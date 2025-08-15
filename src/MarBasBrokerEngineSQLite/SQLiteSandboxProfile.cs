using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Broker;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CraftedSolutions.MarBasBrokerEngineSQLite
{
    public sealed class SQLiteSandboxProfile(ISandboxManager sandboxManager, IBrokerContext context, IConfiguration configuration, IHostEnvironment environment, ILogger<SQLiteSandboxProfile> logger)
        : SQLiteProfile(configuration, environment, logger)
    {
        private readonly ISandboxManager _sandboxManager = sandboxManager;
        private readonly IBrokerContext _context = context;

        public override bool IsOnline => IsOnlineAsync().Result;

        protected override SqliteConnectionStringBuilder ConnectionSettings
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionSettings.DataSource))
                {
                    var sandboxName = CurrentSandboxName;
                    _connectionSettings.DataSource = _sandboxManager.AcquireSandbox(sandboxName);
                    _connectionSettings.Pooling = _configuration.GetValue("BrokerProfile:Pooling", true);

                    var dirPath = Path.GetDirectoryName(_connectionSettings.DataSource);
                    if (string.IsNullOrEmpty(dirPath))
                    {
                        throw new ApplicationException($"Cannot use data source {_connectionSettings.DataSource}");
                    }
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }
                }
                return _connectionSettings;

            }
        }

        protected override bool IsSilentInit
        {
            get
            {
                return _sandboxManager.IsAvailable(CurrentSandboxName);
            }
        }

        public override async Task<bool> IsOnlineAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await InitServiceAsync(cancellationToken);
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(e, "Error reading profile status");
                }
            }
            return false;
        }

        private string CurrentSandboxName => Convert.ToHexString(Encoding.UTF8.GetBytes(_context.User.Identity?.Name ?? SchemaDefaults.SystemUserName));
    }
}
