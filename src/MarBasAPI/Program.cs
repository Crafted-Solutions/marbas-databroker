using CraftedSolutions.MarBasAPICore.Extensions;
using CraftedSolutions.MarBasAPICore.Http;
using CraftedSolutions.MarBasAPICore.Routing;
using Microsoft.AspNetCore.Authentication;

namespace CraftedSolutions.MarBasAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap.Add("DownloadDisposition", typeof(DownloadDispositionRouteConstraint));
            });

            using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConfiguration(
                builder.Configuration.GetSection("Logging")).AddConsole().AddDebug().AddEventSourceLogger()
                );
            var bootstrapLogger = loggerFactory.CreateLogger<Program>();

            builder.Services.ConfigureMarBasTimeouts(builder.Configuration.GetSection("RequestTimeouts"));
            builder.Services.ConfigureMarBasControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.ConfigureMarBasSwagger(builder.Environment.IsDevelopment(), options =>
            {
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{nameof(MarBasAPI)}.xml"));
            });
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, DevelBasicAuthHandler>("BasicAuthentication", null);
            }
            builder.Services.AddHttpContextAccessor();

            var corsEnabled = builder.Services.ConfigureCors(builder.Configuration.GetSection("Cors"), bootstrapLogger);
            var asyncInitServices = builder.Services.RegisterServices(builder.Configuration.GetSection("Services"), bootstrapLogger);
            if (asyncInitServices.Any())
            {
                builder.Services.RegisterAsyncInitService().AddMultipleInitServices(asyncInitServices);
            }

            var app = builder.Build();
            app.UseRequestTimeouts();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.DisplayRequestDuration();
                });
            }

            app.UseHttpsRedirection();

            if (corsEnabled)
            {
                app.UseCors();
            }
            app.UseAuthentication();
            app.UseAuthorization();

            if (builder.Configuration.GetValue("StaticFiles:Enabled", false))
            {
                app.UseStaticFiles();
            }

            app.MapControllers();

            app.Run();
        }
    }
}