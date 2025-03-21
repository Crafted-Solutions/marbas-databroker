using System.Text.Json.Serialization;
using CraftedSolutions.MarBasAPICore.Http;
using CraftedSolutions.MarBasAPICore.Swagger;
using CraftedSolutions.MarBasCommon.Json;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.IO;
using CraftedSolutions.MarBasSchema.Transport;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CraftedSolutions.MarBasAPICore.Extensions
{
    public static class WebApplicationBuilderExtension
    {
        public static IMvcBuilder ConfigureMarBasControllers(this IServiceCollection services, Action<MvcOptions>? configure = null)
        {
            var result = services.AddControllers((options) =>
            {
                options.Filters.Add<HttpResponseExceptionFilter>();
                configure?.Invoke(options);
            });
            result.AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new IsoDateTimeJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new Base64JsonConverter());
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new InterfaceJsonConverter<ITypeConstraint, SimpleTypeConstraint>());
                options.JsonSerializerOptions.Converters.Add(new InterfaceJsonConverter<IAclEntryTransportable, AclEntryTransportable>());
                options.JsonSerializerOptions.Converters.Add(new InterfaceJsonConverter<IGrainLocalizedLayer, GrainLocalizedLayer>());
                options.JsonSerializerOptions.Converters.Add(new InterfaceJsonConverter<IStreamableContent, StreamableContent>());
            });
            return result;
        }

        public static IServiceCollection ConfigureMarBasSwagger(this IServiceCollection services, IConfiguration configuration, Action<SwaggerGenOptions>? setupAction = null)
        {
            var result = services.AddSwaggerGen((options) =>
            {
                options.OrderActionsBy((api) =>
                {
                    var orderPfx = (null == api.ActionDescriptor.AttributeRouteInfo?.Order ? 9999 : api.ActionDescriptor.AttributeRouteInfo?.Order).ToString()?.PadLeft(4, '0');
                    return $"{orderPfx}_{api.ActionDescriptor.RouteValues["controller"]}";
                });
                options.SchemaFilter<EnumSchemaFilter>();
                options.OperationFilter<OptionalRouteParameterOperationFilter>();
                var docPath = Path.Combine(AppContext.BaseDirectory, $"{nameof(MarBasAPICore)}.xml");
                if (File.Exists(docPath))
                {
                    options.IncludeXmlComments(docPath);
                }

                var authSchema = configuration.GetValue("Auth:Schema", "Basic");
                if ("Basic" == authSchema)
                {
                    options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
                    {
                        Description = "Basic auth authentication",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Scheme = "basic",
                        Type = SecuritySchemeType.Http
                    });
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Basic" }
                            },
                            new List<string>()
                        }
                    });
                }
                else
                {
                    // TODO JWT
                    throw new NotImplementedException($"Unknown authentication schema: ${authSchema}");
                }

                setupAction?.Invoke(options);
            });
            return result;
        }

        public static IServiceCollection ConfigureMarBasTimeouts(this IServiceCollection services, IConfiguration configuration)
        {
            var result = services.AddRequestTimeouts(options =>
            {
                options.DefaultPolicy = new RequestTimeoutPolicy { Timeout = TimeSpan.FromSeconds(configuration.GetValue("Default", 120)) };
                options.AddPolicy("FileDownload", TimeSpan.FromSeconds(configuration.GetValue("FileDownload", 300)));
                options.AddPolicy("FileUpload", TimeSpan.FromSeconds(configuration.GetValue("FileUpload", 300)));
                options.AddPolicy("Import", TimeSpan.FromSeconds(configuration.GetValue("Import", 360)));
                options.AddPolicy("Export", TimeSpan.FromSeconds(configuration.GetValue("Export", 360)));
            });
            return result;
        }

        public static AuthenticationBuilder ConfigureMarBasAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var schema = configuration.GetValue("Schema", "Basic");
            if ("Basic" == schema)
            {
                return services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, DevelBasicAuthHandler>("BasicAuthentication", null);
            }
            else
            {
                // TODO JWT
                throw new NotImplementedException($"Unknown authentication schema: ${schema}");
            }
        }
    }
}
