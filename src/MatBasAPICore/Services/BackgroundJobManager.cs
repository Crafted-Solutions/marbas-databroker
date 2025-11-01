using CraftedSolutions.MarBasCommon.Job;
using CraftedSolutions.MarBasCommon.Reflection;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Broker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Security.Principal;
using Timer = System.Timers.Timer;

namespace CraftedSolutions.MarBasAPICore.Services
{
    public sealed class BackgroundJobManager: IBackgroundJobManager, IDisposable
    {
        private readonly object _lock = new();
        private bool _disposed;

        private readonly Dictionary<Guid, IBackgroundJob> _jobs = [];
        private readonly Timer _cleanUpTimer;
        private readonly TimeSpan _cancelJobsAfter;
        private readonly TimeSpan? _cancelCriticalJobsAfter;
        private readonly TimeSpan _keepInactiveJobsFor;
        private readonly IServiceProvider _services;
        private readonly ILogger<BackgroundJobManager> _logger;

        public BackgroundJobManager(IServiceProvider services, IConfiguration configuration, ILogger<BackgroundJobManager> logger)
        {
            _services = services;
            _logger = logger;
            if (!TimeSpan.TryParse(configuration.GetValue("BackgroundJobs:CancelJobsAfter", "00:05:00"), CultureInfo.InvariantCulture, out _cancelJobsAfter))
            {
                _cancelJobsAfter = 5.min();
            }
            if (TimeSpan.TryParse(configuration.GetValue<string>("BackgroundJobs:CancelCriticalJobsAfter"), CultureInfo.InvariantCulture, out var timeSpan))
            {
                _cancelCriticalJobsAfter = timeSpan;
            }
            if (!TimeSpan.TryParse(configuration.GetValue("BackgroundJobs:KeepInactiveJobsFor", "00:10:00"), CultureInfo.InvariantCulture, out _keepInactiveJobsFor))
            {
                _keepInactiveJobsFor = 10.min();
            }
            _cleanUpTimer = new(configuration.GetValue("BackgroundJobs:AutoCleanUpInterval", Math.Min(_cancelJobsAfter.Ticks, 5000)))
            {
                AutoReset = true,
                Enabled = false
            };
            _cleanUpTimer.Elapsed += (_,_) => CleanUpJobs();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cleanUpTimer.Dispose();
                foreach (var job in _jobs.Values)
                {
                    job.Dispose();
                }
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        public IBackgroundJob? GetJob(Guid id, bool removeIfInactive = false)
        {
            lock (_lock)
            {
                if (_jobs.TryGetValue(id, out var result))
                {
                    CheckAccess(result);
                    if (removeIfInactive && BackgroundJobStatus.Complete <= result.Status)
                    {
                        _jobs.Remove(id);
                        if (0 == _jobs.Count)
                        {
                            _cleanUpTimer.Enabled = false;
                        }
                    }
                    return result;
                }
                return null;
            }
        }

        public IBackgroundJob EmplaceJob(string name, string? owner = null, BackgroundJobFlags flags = BackgroundJobFlags.None, string stage = "Default")
        {
            if (null == owner)
            {
                owner = GetCurrentUser().Identity?.Name ?? string.Empty;
            }
            return AddJob(new BackgroundJob(name, owner, flags, stage));
        }

        public IBackgroundJob AddJob(IBackgroundJob newJob)
        {
            if (string.IsNullOrEmpty(newJob.Owner))
            {
                throw new ArgumentException($"{nameof(IBackgroundJob.Owner)} is required");
            }
            lock (_lock)
            {
                if (_jobs.ContainsKey(newJob.Id))
                {
                    throw new ArgumentException($"Job {newJob.Id} exists already");
                }
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Adding job {name} for {owner}", newJob.Name, newJob.Owner);
                }
                _jobs[newJob.Id] = newJob;
                if (!_cleanUpTimer.Enabled)
                {
                    _cleanUpTimer.Enabled = true;
                }
                return newJob;
            }
        }

        public IBackgroundJob? RemoveJob(Guid id, bool cancelRemoved = false)
        {
            lock(_lock)
            {
                if (_jobs.TryGetValue(id, out var result))
                {
                    CheckAccess(result);
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Removing job {id} (while cancelling: {cancel})", id, cancelRemoved);
                    }
                    _jobs.Remove(id);
                    if (cancelRemoved && BackgroundJobStatus.Complete > result.Status)
                    {
                        result.Status = BackgroundJobStatus.Cancelled;
                    }
                    if (0 == _jobs.Count)
                    {
                        _cleanUpTimer.Enabled = false;
                    }
                    return result;
                }
                return null;
            }
        }

        public IEnumerable<IBackgroundJob> ListJobs(bool forAllUsers = false)
        {
            if (forAllUsers && !CheckAllJobsAccess())
            {
                throw new UnauthorizedAccessException($"Current user lacks entitlement to see all jobs");
            }
            lock (_lock)
            {
                if (forAllUsers)
                {
                    return [.. _jobs.Values];
                }
                var currentUser = GetCurrentUser();
                return [.. _jobs.Values.Where(x => x.Owner == currentUser.Identity?.Name)];
            }
        }

        private void CheckAccess(IBackgroundJob job)
        {
            var user = GetCurrentUser();
            if (job.Owner != user.Identity?.Name)
            {
                throw new UnauthorizedAccessException($"User '{user.Identity?.Name}' doesn't own the job {job.Id}");
            }
        }

        private IPrincipal GetCurrentUser()
        {
            using (var scope = _services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IBrokerContext>();
                return context.User;
            }
        }

        private bool CheckAllJobsAccess()
        {
            using (var scope = _services.CreateScope())
            {
                var accessService = scope.ServiceProvider.GetRequiredService<IAccessService>();
                return accessService.VerifyRoleEntitlement(RoleEntitlement.SkipPermissionCheck, true);
            }
        }

        private void CleanUpJobs()
        {
            lock (_lock)
            {
                if (0 == _jobs.Count)
                {
                    _cleanUpTimer.Enabled = false;
                    return;
                }
                var now = DateTime.UtcNow;
                // cancel jobs running for too long
                foreach (var job in _jobs.Values.Where(x => BackgroundJobStatus.Pending < x.Status && BackgroundJobStatus.Complete > x.Status))
                {
                    var compareTs = job.Flags.HasFlag(BackgroundJobFlags.Critical) ? _cancelCriticalJobsAfter : _cancelJobsAfter;
                    if (null != compareTs && now - job.Started > compareTs)
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("Automatically cancelling job {id} after {runtime} runnig time", job.Id, now - job.Started);
                        }
                        job.Status = BackgroundJobStatus.Cancelled;
                    }
                }
                // remove inactive jobs
                var junkJobs = _jobs.Where(x => BackgroundJobStatus.Complete <= x.Value.Status && now - x.Value.Created > _keepInactiveJobsFor).Select(x => x.Key).ToList();
                if (_logger.IsEnabled(LogLevel.Debug) && 0 < junkJobs.Count)
                {
                    _logger.LogDebug("Removing jobs inactive for longer than {maxage}: {jobs}", _keepInactiveJobsFor, string.Join(", ", junkJobs));
                }
                foreach (var id in junkJobs)
                {
                    _jobs.Remove(id, out var job);
                    job?.Dispose();
                }
                if (0 == _jobs.Count)
                {
                    _cleanUpTimer.Enabled = false;
                }
            }
        }
    }
}
