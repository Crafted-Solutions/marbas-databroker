using CraftedSolutions.MarBasCommon.Job;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasAPICore.Services
{
    public sealed class BackgroundQueueProcessor(IBackgroundWorkQueue taskQueue, ILogger<BackgroundQueueProcessor> logger) : BackgroundService
    {
        private readonly ILogger<BackgroundQueueProcessor> _logger = logger;

        public IBackgroundWorkQueue TaskQueue { get; } = taskQueue;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Starting background queue");
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(stoppingToken); // waits until work items available
                try
                {
                    await workItem(stoppingToken);
                }
                catch (Exception e) // work items should never throw but safe is safe
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError(e, "Unexpected error occured executing {workItem}", nameof(workItem));
                    }
                }
            }
        }
    }
}
