using CraftedSolutions.MarBasAPICore.Services;
using CraftedSolutions.MarBasCommon.Job;
using Microsoft.Extensions.DependencyInjection;

namespace CraftedSolutions.MarBasAPICore.Extensions
{
    public static class BackgroundQueueExtension
    {
        public static IServiceCollection ConfigureBackgroundQueue(this IServiceCollection services)
        {
            services.AddHostedService<BackgroundQueueProcessor>();
            services.AddSingleton<IBackgroundWorkQueue, BackgroundWorkQueue>();
            services.AddSingleton<IBackgroundJobManager, BackgroundJobManager>();
            return services;
        }
    }
}
