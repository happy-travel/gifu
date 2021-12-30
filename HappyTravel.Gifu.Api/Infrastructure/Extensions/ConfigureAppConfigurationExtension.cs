using System;
using HappyTravel.ConsulKeyValueClient.ConfigurationProvider.Extensions;
using HappyTravel.Gifu.Api.Infrastructure.Environment;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions;

public static class ConfigureAppConfigurationExtension
{
    public static void ConfigureAppConfiguration(this WebApplicationBuilder builder)
    {
        var environment = builder.Environment;
        var consulAddress = System.Environment.GetEnvironmentVariable("CONSUL_HTTP_ADDR");
        var consulToken = System.Environment.GetEnvironmentVariable("CONSUL_HTTP_TOKEN");
        
        ArgumentNullException.ThrowIfNull(consulAddress);
        ArgumentNullException.ThrowIfNull(consulToken);

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddConsulKeyValueClient( consulAddress,
                "gifu",
                consulToken,
                environment.EnvironmentName,
                environment.IsLocal());
    }
}