using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusUploader.Http;
using NexusUploader.Services;
using Spectre.Cli;
using Spectre.Cli.AppInfo;
using Spectre.Cli.Extensions.DependencyInjection;

namespace NexusUploader
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var confOverride = Environment.GetEnvironmentVariable("UNEX_CONFIG");
             IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddFile("unex")
                .AddEnvironmentVariables("UNEX_");
            if (confOverride.IsSet() && System.IO.File.Exists(confOverride)) {
                configBuilder.AddFile(confOverride);
            }
            IConfiguration config = configBuilder.Build();
            var opts = config.Get<ModConfiguration>();
            var services = new ServiceCollection();
            services.AddLogging(logging => {
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddInlineSpectreConsole(c => {
                    c.LogLevel = GetLogLevel();
                });
                logging.AddFilter("System.Net.Http", LogLevel.Warning);
            });
            services.AddNexusApiClient();
            services.AddNexusClient();
            services.AddUploadClient();
            services.AddSingleton<ModConfiguration>(opts ?? new ModConfiguration());
            services.AddSingleton<CookieService>();
            // services.AddSingleton<ILoggingConfiguration>(GetDefaultLogging());
            var app = new CommandApp(new DependencyInjectionRegistrar(services));
            app.Configure(c => {
                c.SetApplicationName("unex");
                if (GetLogLevel() < LogLevel.Information) {
                    c.PropagateExceptions();
                }
                c.AddCommand<InfoCommand>("info");
                c.AddCommand<ChangelogCommand>("changelog");
                c.AddCommand<UploadCommand>("upload");
                c.AddCommand<CheckCommand>("check");
            });
            return await app.RunAsync(args);
        }

        private static LogLevel GetLogLevel() {
            var envVar = System.Environment.GetEnvironmentVariable("UNEX_DEBUG");
            return string.IsNullOrWhiteSpace(envVar) 
                ? LogLevel.Information
                :  envVar.ToLower() == "trace"
                    ? LogLevel.Trace
                    : LogLevel.Debug;
        }
    }
}
