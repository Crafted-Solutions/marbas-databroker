using CraftedSolutions.MarBasCommon.DependencyInjection;
using CraftedSolutions.MarBasCommon.Reflection;
using CraftedSolutions.MarBasSchema.Access;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Globalization;

namespace CraftedSolutions.MarBasBrokerEngineSQLite
{
    public sealed class SQLiteSandboxManager
        : ISandboxManager, IAsyncInitService, IDisposable
    {
        private readonly ConcurrentDictionary<string, DateTime> _sandboxes = [];

        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<SQLiteSandboxManager> _logger;
        private readonly string _sandboxDirectory;
        private readonly string _sanboxFilenamePattern;
        private readonly string? _sandboxTemplate;
        private readonly string? _templateProfile;
        private readonly TimeSpan _sandboxMaxAge;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private DateTime _lastTrimTS;

        private bool _disposed;

        public SQLiteSandboxManager(IConfiguration configuration, IHostEnvironment environment, ILogger<SQLiteSandboxManager> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
            (_sandboxDirectory, _sanboxFilenamePattern) = GetSandboxDataSourceConfig();
            var tplConf = _configuration.GetValue<string>("BrokerProfile:SanboxTemplate");
            if (!string.IsNullOrEmpty(tplConf))
            {
                _sandboxTemplate = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, Environment.ExpandEnvironmentVariables(tplConf)));
            }
            _templateProfile = _configuration.GetValue<string>("BrokerProfile:SanboxTemplateProfile");
            if (!TimeSpan.TryParse(_configuration.GetValue("BrokerProfile:SanboxMaxAge", "12:00:00"), CultureInfo.InvariantCulture, out _sandboxMaxAge))
            {
                 _sandboxMaxAge = 12.h();
            }
            _lastTrimTS = DateTime.Now - _sandboxMaxAge - 1.h();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _semaphore.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        public int SandboxCount => _sandboxes.Count;

        public int SandboxMaxCount => _configuration.GetValue("BrokerProfile:UserSanboxes", -1);

        public async Task<bool> InitServiceAsync(CancellationToken cancellationToken = default)
        {
            if (0 >= SandboxMaxCount)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("BrokerProfile:UserSanboxes must be greater than 0, disabling sandboxing");
                }
                return false;
            }
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Initializing sandbox manager for {sandboxMaxCount} users", SandboxMaxCount);
            }
            if (Directory.Exists(_sandboxDirectory))
            {
                foreach (var path in Directory.EnumerateFiles(_sandboxDirectory, "*.sqlite", new EnumerationOptions() { IgnoreInaccessible = true }))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }
                    if (path == _sandboxTemplate)
                    {
                        continue;
                    }
                    _sandboxes[Path.GetFileName(path)] = File.GetCreationTime(path);
                }
                await TrimOldest(cancellationToken);
            }
            return true;
        }

        public async Task<bool> TrimOldest(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                return await Task.Run(() =>
                {
                    SqliteConnection.ClearAllPools();
                    var remaining = new Dictionary<string, DateTime>();
                    foreach (var path in Directory.EnumerateFiles(_sandboxDirectory, "*.sqlite", new EnumerationOptions() { IgnoreInaccessible = true }))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return false;
                        }
                        var name = Path.GetFileName(path);
                        if (_sandboxes.ContainsKey(name))
                        {
                            var ct = File.GetCreationTime(path);
                            if (DateTime.Now - ct >= _sandboxMaxAge)
                            {
                                if (_logger.IsEnabled(LogLevel.Information))
                                {
                                    _logger.LogInformation("Deleting expired sandbox {path}", path);
                                }
                                if (_sandboxes.ContainsKey(name))
                                {
                                    _ = _sandboxes.TryRemove(name, out _);
                                }
                                File.Delete(path);
                            }
                            else
                            {
                                remaining[path] = File.GetLastAccessTime(path);
                            }
                        }
                        else if (path != _sandboxTemplate)
                        {
                            if (_logger.IsEnabled(LogLevel.Information))
                            {
                                _logger.LogInformation("Deleting unlisted sandbox {path}", path);
                            }
                            File.Delete(path);
                        }
                    }

                    if (SandboxCount > SandboxMaxCount)
                    {
                        var oldest = remaining.OrderBy(x => x.Value).ToList();
                        for (var i = 0; i < SandboxCount - SandboxMaxCount && i < oldest.Count; i++)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return false;
                            }
                            if (_logger.IsEnabled(LogLevel.Information))
                            {
                                _logger.LogInformation("Deleting surplus sandbox {path}", oldest[i].Key);
                            }
                            if (_sandboxes.ContainsKey(oldest[i].Key))
                            {
                                _ = _sandboxes.TryRemove(oldest[i].Key, out _);
                            }
                            File.Delete(oldest[i].Key);
                        }
                    }
                    _lastTrimTS = DateTime.Now;

                    return SandboxCount <= SandboxMaxCount;
                }, cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public bool IsAvailable(string sandboxName)
        {
            return _sandboxes.TryGetValue(string.Format(_sanboxFilenamePattern, sandboxName), out var ct) && DateTime.Now - ct < _sandboxMaxAge;
        }

        public string AcquireSandbox(string sandboxName)
        {
            return AcquireSandboxAsync(sandboxName).Result;
        }

        public Task<string> AcquireSandboxAsync(string sandboxName, CancellationToken cancellationToken = default)
        {
            _semaphore.Wait(cancellationToken);
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled<string>(cancellationToken);
                }
                var templateExists = !string.IsNullOrEmpty(_sandboxTemplate) && File.Exists(_sandboxTemplate);
                if (templateExists && sandboxName == _templateProfile)
                {
                    return Task.FromResult(_sandboxTemplate!);
                }

                var fileName = string.Format(_sanboxFilenamePattern, sandboxName);
                var result = Path.Combine(_sandboxDirectory, fileName);
                if (_sandboxes.TryGetValue(fileName, out var ct))
                {
                    if (DateTime.Now - ct >= _sandboxMaxAge)
                    {
                        SqliteConnection.ClearAllPools();
                        if (File.Exists(result))
                        {
                            File.Delete(result);
                        }
                        _ = _sandboxes.TryRemove(fileName, out _);
                        throw new SandboxException($"Sandbox {sandboxName} is too old: {DateTime.Now - ct} >= {_sandboxMaxAge}");
                    }
                }
                else
                {
                    if (SandboxCount >= SandboxMaxCount)
                    {
                        throw new SandboxException($"All {SandboxMaxCount} sandbox slots are taken, cannot acquire {sandboxName}");
                    }
                    if (templateExists)
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Initializing profile {result} from template {template}", result, _sandboxTemplate);
                        }
                        File.Copy(_sandboxTemplate!, result, true);
                        File.SetCreationTime(result, DateTime.Now);
                    }
                    _sandboxes[fileName] = File.Exists(result) ? File.GetCreationTime(result) : DateTime.Now;
                }
                return Task.FromResult(result);

            }
            finally
            {
                _semaphore.Release();
            }
        }

        private (string, string) GetSandboxDataSourceConfig()
        {
            var datasource = Path.Combine(_environment.ContentRootPath, Environment.ExpandEnvironmentVariables(_configuration.GetValue("BrokerProfile:DataSource", "%TEMP%/marbas-databroker/{0}.sqlite")));
            var dir = Path.GetDirectoryName(datasource);
            if (string.IsNullOrEmpty(dir) || dir.Contains("{0}"))
            {
                throw new ApplicationException($"Setting BrokerProfile:DataSource {datasource} is not suitable for sandboxing");
            }
            dir = Path.GetFullPath(dir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return (dir, Path.GetFileName(datasource));
        }
    }
}
