using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CraftedSolutions.MarBasAPICore.Extensions
{
    public static class LoggingExtension
    {
        public static ILoggerFactory GetBootstrapLoggerFactory(this IHostApplicationBuilder builder)
        {
            return LoggerFactory.Create(loggingBuilder =>
                loggingBuilder.AddConfiguration(builder.Configuration.GetSection("Logging"))
                    .AddConsole()
                    .AddDebug()
                    .AddEventSourceLogger());
        }

        public static void ConfigureTraceFileLogging(this IHostApplicationBuilder builder)
        {
            var logPath = builder.Configuration.GetValue("Logging:TraceFile:Path", string.Empty);
            if (!string.IsNullOrEmpty(logPath))
            {
                logPath = Environment.ExpandEnvironmentVariables(Path.Combine(builder.Environment.ContentRootPath, logPath));
                
                builder.Logging.AddTraceSource(new SourceSwitch("TraceFile", builder.Configuration.GetValue("Logging:TraceFile:LogLevel", "Information"))
                    , new TextWriterTraceListener(logPath) { TraceOutputOptions = TraceOptions.DateTime });
                if (builder.Configuration.GetValue("Logging:TraceFile:AutoFlush", false))
                {
                    Trace.AutoFlush = true;
                }
            }
        }
    }
}
