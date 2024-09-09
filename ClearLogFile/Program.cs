using ClearLogFile.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClearLogFile
{
    class Program
    {
        static void Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();

            host.Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            IHostEnvironment env = hostContext.HostingEnvironment;
            IConfiguration configuration = hostContext.Configuration;

            var config = new ConfigurationBuilder().SetBasePath(System.IO.Directory.GetCurrentDirectory())
                                                   .AddJsonFile("appsettings.json", optional: false, true)
                                                   .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, true)
                                                   .AddEnvironmentVariables()
                                                   //.AddJsonFile("nlog.json", optional: true, reloadOnChange: true)
                                                   .Build();
            services.AddSingleton(configuration.GetSection("Application").Get<ConfigSettings>());

            services.AddHostedService<ClearLogFile>();

            services.AddLogging(logconfig =>
            {
                logconfig.ClearProviders();
                logconfig.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                logconfig.AddConsole();
                //logconfig.AddNLog(config);
            });

            var serviceProvider = services.BuildServiceProvider();
            services.AddSingleton(typeof(ILogger), serviceProvider.GetRequiredService<ILogger<ClearLogFile>>());

        }).UseWindowsService();

    }
}