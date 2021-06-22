using System;
using HappyTravel.ConsulKeyValueClient.ConfigurationProvider.Extensions;
using HappyTravel.Gifu.Api.Infrastructure.Environment;
using HappyTravel.StdOutLogger.Extensions;
using HappyTravel.StdOutLogger.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Gifu.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseKestrel()
                        .UseDefaultServiceProvider(s =>
                        {
                            s.ValidateScopes = true;
                            s.ValidateOnBuild = true;
                        });
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var environment = hostingContext.HostingEnvironment;

                    config
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddConsulKeyValueClient(Environment.GetEnvironmentVariable("CONSUL_HTTP_ADDR") ?? throw new InvalidOperationException("Consul endpoint is not set"),
                        "gifu",
                        Environment.GetEnvironmentVariable("CONSUL_HTTP_TOKEN") ?? throw new InvalidOperationException("Consul http token is not set"));
                    
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders()
                        .AddConfiguration(hostingContext.Configuration.GetSection("Logging"));

                    var env = hostingContext.HostingEnvironment;
                    if (env.IsLocal())
                        logging.AddConsole();
                    else
                    {
                        logging.AddConsole();
                        logging.AddStdOutLogger(setup =>
                        {
                            setup.IncludeScopes = true;
                            setup.RequestIdHeader = Constants.DefaultRequestIdHeader;
                            setup.UseUtcTimestamp = true;
                        });
                        logging.AddSentry(c =>
                        {
                            c.Dsn = EnvironmentVariableHelper.Get("Sentry:Endpoint", hostingContext.Configuration);
                            c.Environment = env.EnvironmentName;
                        });
                    }
                });
    }
}
