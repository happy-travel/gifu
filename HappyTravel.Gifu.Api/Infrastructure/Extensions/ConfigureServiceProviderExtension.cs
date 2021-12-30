using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions;

public static class ConfigureServiceProviderExtension
{
    public static void ConfigureServiceProvider(this WebApplicationBuilder builder)
    {
        builder.WebHost.UseDefaultServiceProvider(o =>
        {
            o.ValidateScopes = true;
            o.ValidateOnBuild = true;
        });
    }
}