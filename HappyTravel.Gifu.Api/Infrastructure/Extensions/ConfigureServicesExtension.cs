using System;
using HappyTravel.ErrorHandling.Extensions;
using HappyTravel.Gifu.Api.Infrastructure.Environment;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Services;
using HappyTravel.Gifu.Api.Services.SupplierServices.IxarisServices;
using HappyTravel.Gifu.Api.Services.VccServices;
using HappyTravel.Gifu.Data;
using HappyTravel.Telemetry.Extensions;
using HappyTravel.VaultClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions;

public static class ConfigureServicesExtension
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        using var vaultClient = new VaultClient.VaultClient(new VaultOptions
        {
            BaseUrl = new Uri(EnvironmentVariableHelper.Get("Vault:Endpoint", builder.Configuration)),
            Engine = builder.Configuration["Vault:Engine"],
            Role = builder.Configuration["Vault:Role"]
        });
            
        vaultClient.Login(EnvironmentVariableHelper.Get("Vault:Token", builder.Configuration)).GetAwaiter().GetResult();

        builder.Services
            .AddMvcCore()
            .AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.AddPolicy("CanIssue", policy =>
                {
                    policy.RequireClaim("scope", "vcc.issue");
                });
                options.AddPolicy("CanGetHistory", policy =>
                {
                    policy.RequireClaim("scope", "vcc.history");
                });
                options.AddPolicy("CanEdit", policy =>
                {
                    policy.RequireClaim("scope", "vcc.edit");
                });
            })
            .AddApiExplorer();

        builder.Services.AddHealthChecks()
            .AddDbContextCheck<GifuContext>();

        builder.Services.Configure<UserDefinedFieldsOptions>(builder.Configuration.GetSection("UserDefinedFieldsOptions"));
        builder.Services.Configure<DirectEditOptions>(builder.Configuration.GetSection("DirectEditOptions"));
        builder.Services.Configure<FakeAmexCardOptions>(builder.Configuration.GetSection("FakeAmexCardOptions"));

        builder.Services
            .AddProblemDetailsErrorHandling()
            .AddHttpContextAccessor()
            .ConfigureApiVersioning()
            .ConfigureSwagger()
            .ConfigureDatabaseOptions(vaultClient, builder.Configuration)
            .ConfigureAuthentication(vaultClient, builder.Configuration)
            .ConfigureAmExIssuer(vaultClient, builder.Configuration)  
            .ConfigureIxarisIssuer(vaultClient, builder.Configuration)
            .ConfigureVccServiceResolver()
            .AddTransient<VccServiceResolver>()
            .AddTransient<IVccService, VccService>()
            .AddTransient<IClientService, ClientService>()
            .AddTransient<ICustomFieldsMapper, CustomFieldsMapper>()
            .AddTransient<IVccIssueRecordsManager, VccIssueRecordsManager>()
            .AddTransient<IAccountsService, AccountService>()
            .AddTransient<IVccFactoryService, VccFactoryService>()
            .AddTransient<IScheduleLoadRecordsManager, ScheduleLoadRecordsManager>()
            .AddTracing(builder.Configuration, options =>
            {
                options.ServiceName = $"{builder.Environment.ApplicationName}-{builder.Environment.EnvironmentName}";
                options.JaegerHost = builder.Environment.IsLocal()
                    ? builder.Configuration.GetValue<string>("Jaeger:AgentHost")
                    : builder.Configuration.GetValue<string>(builder.Configuration.GetValue<string>("Jaeger:AgentHost"));
                options.JaegerPort = builder.Environment.IsLocal()
                    ? builder.Configuration.GetValue<int>("Jaeger:AgentPort")
                    : builder.Configuration.GetValue<int>(builder.Configuration.GetValue<string>("Jaeger:AgentPort"));
            });
    }
}