using CraftedSolutions.MarBasCommon.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasAPICore.Services
{
    public class InitHostedService(IInitializerService initializerService, IServiceProvider serviceProvider, ILogger<InitHostedService> logger) : IHostedService
    {
        protected readonly ILogger _logger = logger;
        protected readonly IInitializerService _initializerService = initializerService;
        protected readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Initializing asynchronous services");
            }
            await _initializerService.InitializeServicesAsync(_serviceProvider, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
