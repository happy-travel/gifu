using HappyTravel.Gifu.Api.Infrastructure.Environment;
using HappyTravel.StdOutLogger.Extensions;
using HappyTravel.StdOutLogger.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions;

public static class ConfigureLoggingExtension
{
    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders()
            .AddConfiguration(builder.Configuration.GetSection("Logging"));
        
        if (builder.Environment.IsLocal())
            builder.Logging.AddConsole();
        else
        {
            builder.Logging.AddStdOutLogger(setup =>
            {
                setup.IncludeScopes = true;
                setup.RequestIdHeader = Constants.DefaultRequestIdHeader;
                setup.UseUtcTimestamp = true;
            });
            builder.Logging.AddSentry();
        }
    }
}