using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasAPICore.Extensions
{
    public static class ConfigurableCorsExtension
    {
        public static bool ConfigureCors(this IServiceCollection services, IConfiguration configuration, ILogger? logger = null)
        {
            if (configuration.GetValue("Enabled", false))
            {
                logger?.LogInformation("Enabling CORS");
                services.AddCors(options =>
                {
                    var builder = (CorsPolicyBuilder policyBuilder, IConfigurationSection policyConfig) =>
                    {
                        var val = policyConfig.GetValue<string>("AllowedOrigins");
                        if ("*".Equals(val, StringComparison.OrdinalIgnoreCase))
                        {
                            policyBuilder.AllowAnyOrigin();
                        }
                        else if (!string.IsNullOrEmpty(val))
                        {
                            policyBuilder.WithOrigins(val.Split(","));
                        }
                        val = policyConfig.GetValue<string>("AllowedMethods");
                        if (string.IsNullOrEmpty(val) || "*".Equals(val, StringComparison.OrdinalIgnoreCase))
                        {
                            policyBuilder.AllowAnyMethod();
                        }
                        else
                        {
                            policyBuilder.WithMethods(val.Split(","));
                        }
                        val = policyConfig.GetValue<string>("AllowedHeaders");
                        if ("*".Equals(val, StringComparison.OrdinalIgnoreCase))
                        {
                            policyBuilder.AllowAnyHeader();
                        }
                        else if (!string.IsNullOrEmpty(val))
                        {
                            policyBuilder.WithHeaders(val.Split(","));
                        }

                        if (policyConfig.GetValue<bool>("AllowCredentials"))
                        {
                            policyBuilder.AllowCredentials();
                        }
                    };

                    var policies = configuration.GetSection("Policies").GetChildren();
                    foreach (var policy in policies)
                    {
                        var name = policy.GetValue<string>("Name");
                        if (string.IsNullOrEmpty(name) || "Default".Equals(name, StringComparison.Ordinal))
                        {
                            logger?.LogInformation("Adding default CORS policy");
                            options.AddDefaultPolicy(policyBuilder => builder(policyBuilder, policy));
                        }
                        else
                        {
                            logger?.LogInformation("Adding CORS policy {0}", name);
                            options.AddPolicy(name, policyBuilder => builder(policyBuilder, policy));
                        }
                    }
                });
                return true;
            }
            return false;
        }
    }
}
