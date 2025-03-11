using System.Reflection;
using CraftedSolutions.MarBasCommon.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasAPICore.Extensions
{
    public static class ConfigurableServicesExtension
    {

        public static IEnumerable<Type> RegisterServices(this IServiceCollection services, IConfiguration configuration, ILogger? logger = null)
        {
            var result = new List<Type>();

            var defAsm = typeof(ConfigurableServicesExtension).Assembly;
            var sections = configuration.GetChildren();
            foreach (var section in sections)
            {
                var implName = section.GetValue<string>("Impl");
                if (string.IsNullOrEmpty(implName))
                {
                    logger?.LogError("Missing service implementation name");
                    continue;
                }
                var typeName = section.GetValue<string>("Type");
                if (string.IsNullOrEmpty(typeName))
                {
                    logger?.LogError("Missing service type for {implName}", implName);
                    continue;
                }
                logger?.LogInformation("Registing service {typeName} ({implName})", typeName, implName);

                var asmName = section.GetValue<string>("Assembly");
                var asm = string.IsNullOrEmpty(asmName) ? defAsm : Assembly.Load(asmName);
                if (null == asm) continue;


                var impl = asm.GetType(implName);
                if (null == impl) continue;


                var typeIfaces = impl.FindInterfaces((m, filterCriteria) => Equals(m.FullName, filterCriteria), typeName);
                if (0 == typeIfaces.Length)
                {
                    logger?.LogError("Service {implName} doesn't implement {typeName}", implName, typeName);
                    continue;
                }

                var lt = ServiceLifetime.Transient;
                switch (section.GetValue<string>("Lifetime"))
                {
                    case "Scoped":
                        lt = ServiceLifetime.Scoped;
                        break;

                    case "Singleton":
                        lt = ServiceLifetime.Singleton;
                        break;
                }
                services.Add(new ServiceDescriptor(typeIfaces[0], impl, lt));

                var asyncIfaces = impl.FindInterfaces((m, filterCriteria) => Equals(m.FullName, filterCriteria), typeof(IAsyncInitService).FullName);
                if (0 < asyncIfaces.Length)
                {
                    result.Add(typeIfaces[0]);
                }
            }
            return result;
        }
    }
}
