using MarBasCommon.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarBasAPICore.Services
{
    public class InitHostedService : IHostedService
    {
        protected readonly ILogger _logger;
        protected readonly IInitializerService _initializerService;
        protected readonly IServiceProvider _serviceProvider;

        public InitHostedService(IInitializerService initializerService, IServiceProvider serviceProvider, ILogger<InitHostedService> logger)
        {
            _logger = logger;
            _initializerService = initializerService;
            _serviceProvider = serviceProvider;
        }

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
