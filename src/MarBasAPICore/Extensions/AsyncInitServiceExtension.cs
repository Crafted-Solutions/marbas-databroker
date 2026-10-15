using CraftedSolutions.MarBasAPICore.Services;
using CraftedSolutions.MarBasCommon.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace CraftedSolutions.MarBasAPICore.Extensions
{
    public static class AsyncInitServiceExtension
    {
        public static IInitializerService RegisterAsyncInitService(this IServiceCollection services)
        {
            var result = new InitializerService();
            services.AddSingleton<IInitializerService>(result);
            services.AddHostedService<InitHostedService>();
            return result;
        }
    }
}
