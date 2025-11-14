using CraftedSolutions.MarBasAPICore.Auth;
using CraftedSolutions.MarBasAPICore.Http;
using CraftedSolutions.MarBasAPICore.Models.Transport;
using CraftedSolutions.MarBasAPICore.Swagger;
using CraftedSolutions.MarBasCommon.Json;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.IO;
using CraftedSolutions.MarBasSchema.Transport;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Serialization;

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
                options.JsonSerializerOptions.Converters.Add(new InterfaceJsonConverter<IGrainPackagingOptions, GrainPackagingOptions>());
            });
            services.AddSingleton((services) =>
            {
                return services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
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

                var authConfig = AuthConfig.Bind(configuration.GetSection(configuration.GetValue(AuthConfig.SectionSwitch, AuthConfig.SectionName)), true);
                switch (authConfig)
                {
                    case IBasicAuthConfig basicAuth:
                        options.AddSecurityDefinition(basicAuth.Schema, new OpenApiSecurityScheme
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
                                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = basicAuth.Schema }
                                },
                                new List<string>()
                            }
                        });
                        break;

                    case OIDCAuthConfigBackend oidcConfig:
                        var scheme = new OpenApiSecurityScheme
                        {
                            In = ParameterLocation.Header,
                            Name = "Authorization",
                            Flows = oidcConfig.GenerateFlows(),
                            Type = SecuritySchemeType.OAuth2
                        };
                        if (!string.IsNullOrEmpty(oidcConfig.BearerTokenName))
                        {
                            scheme.Extensions = new Dictionary<string, IOpenApiExtension>()
                            {
                                // Setting x-tokenName to id_token will send response_type=token id_token and the nonce to the auth provider.
                                // x-tokenName also specifieds the name of the value from the response of the auth provider to use as bearer token.
                                { "x-tokenName", new OpenApiString(oidcConfig.BearerTokenName) }
                            };
                        }
                        options.AddSecurityDefinition("oauth2", scheme);

                        options.AddSecurityRequirement(new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference { Id = "oauth2", Type = ReferenceType.SecurityScheme }
                                },
                                oidcConfig.Scopes.Where(x => x.Value).Select(x => x.Key).ToList()
                            }
                        });
                        break;

                    default:
                        throw new NotImplementedException($"Unknown authentication schema: ${authConfig?.Schema}");
                }

                setupAction?.Invoke(options);
            });
            return result;
        }

        public static IApplicationBuilder ConfigureMarBasSwaggerUI(this IApplicationBuilder app, IConfiguration configuration)
        {
            var authConfig = AuthConfig.Bind(configuration.GetSection(configuration.GetValue(AuthConfig.SectionSwitch, AuthConfig.SectionName)), true);
            return app.UseSwagger()
                .UseSwaggerUI(options =>
                {
                    options.DisplayRequestDuration();

                    if (authConfig is OIDCAuthConfigBackend oidcConfig)
                    {
                        options.OAuthClientId(oidcConfig.ClientId);
                        //options.OAuthAppName("Test OIDC");
                        options.OAuthScopes(
                            oidcConfig.Scopes.Where(x => x.Value).Select(x => x.Key).ToArray()
                        );
                        if (!string.IsNullOrEmpty(oidcConfig.ScopeSeparator))
                        {
                            options.OAuthScopeSeparator(oidcConfig.ScopeSeparator);
                        }
                        if (!string.IsNullOrEmpty(oidcConfig.ClientSecret))
                        {
                            options.OAuthClientSecret(oidcConfig.ClientSecret);
                        }
                        if (CapabilitySpec.NA < oidcConfig.PKCE)
                        {
                            options.OAuthUsePkce();
                        }
                    }
                });
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

        public static AuthenticationBuilder ConfigureMarBasAuthentication(this IServiceCollection services, IConfiguration configuration, ILogger? logger = null)
        {
            var authConfig = AuthConfig.Bind(configuration, true);
            logger?.LogInformation("Adding {schema} authentication", authConfig?.Schema);
            if (authConfig is IOIDCAuthConfig oidcConfig && oidcConfig.UseTokenProxy)
            {
                services.AddHttpClient();
            }
            if (authConfig is not IBasicAuthConfig)
            {
                services.AddScoped<IClaimsTransformation, MapClaimsTransformation>();
            }
            return authConfig switch
            {
                IBasicAuthConfig basicAuth => services.AddAuthentication("BasicAuthentication")
                                        .AddScheme<AuthenticationSchemeOptions, DevelBasicAuthHandler>("BasicAuthentication", null),

                OIDCAuthConfigBackend oidcAuth => services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                                        .AddJwtBearer(options =>
                                        {
                                            options.Authority = oidcAuth.Authority;
                                            if (!string.IsNullOrEmpty(oidcAuth.Audience))
                                            {
                                                options.Audience = oidcAuth.Audience;
                                            }
                                            if (false == oidcAuth.RequireHttpsMetadata)
                                            {
                                                options.RequireHttpsMetadata = false;
                                            }
                                            oidcAuth.TokenValidation?.Commit(options.TokenValidationParameters);
                                        }),

                _ => throw new NotImplementedException($"Unknown authentication schema: ${authConfig?.Schema}"),
            };
        }
    }
}
