using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HappyTravel.Gifu.Api.Infrastructure.Environment;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models.AmEx;
using HappyTravel.Gifu.Api.Services;
using HappyTravel.Gifu.Data;
using HappyTravel.VaultClient;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureSwagger(this IServiceCollection services) 
            => services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HappyTravel.Gifu.Api", Version = "v1" });
                
                var xmlCommentsFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommentsFilePath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFileName);
                c.IncludeXmlComments(xmlCommentsFilePath);
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        Array.Empty<string>()
                    }
                });
            });
        
        
        public static IServiceCollection ConfigureApiVersioning(this IServiceCollection services) 
            => services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });


        public static IServiceCollection ConfigureIssuer(this IServiceCollection services, IVaultClient vaultClient, IConfiguration configuration)
        {
            var amExOptions = vaultClient.Get(configuration["AmExOptions"])
                .GetAwaiter().GetResult();
            
            var amExAccounts = vaultClient.Get(configuration["AmexAccounts"])
                .GetAwaiter().GetResult();

            if (configuration.GetValue<bool>("Testing:UseFakeAmexClient"))
                services.AddHttpClient<IAmExClient, FakeAmexClient>();
            else
                services.AddHttpClient<IAmExClient, AmExClient>();

            var accounts = amExAccounts.Select(a => new
                {
                    Currency = Enum.Parse<AmexCurrencies>(a.Key),
                    AccountId = a.Value
                })
                .ToDictionary(a => a.Currency, a => a.AccountId);

            return services.Configure<AmExOptions>(o =>
                {
                    o.Endpoint = amExOptions["endpoint"];
                    o.ClientId = amExOptions["clientId"];
                    o.ClientSecret = amExOptions["clientSecret"];
                    o.Accounts = accounts;
                })
                .AddTransient<IVccService, VccService>();
        }
        
        
        public static IServiceCollection ConfigureDatabaseOptions(this IServiceCollection services, VaultClient.VaultClient vaultClient, 
            IConfiguration configuration)
        {
            var databaseOptions = vaultClient.Get(configuration["Database:Options"]).GetAwaiter().GetResult();
            
            return services.AddDbContextPool<GifuContext>(options =>
            {
                var host = databaseOptions["host"];
                var port = databaseOptions["port"];
                var password = databaseOptions["password"];
                var userId = databaseOptions["userId"];
            
                var connectionString = configuration["Database:ConnectionString"];
                options.UseNpgsql(string.Format(connectionString, host, port, userId, password), builder =>
                {
                    builder.EnableRetryOnFailure();
                });
                options.UseInternalServiceProvider(null);
                options.EnableSensitiveDataLogging(false);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }, 16);
        }
        
        
        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IVaultClient vaultClient, IConfiguration configuration)
        {
            var authorityOptions = vaultClient.Get(configuration["AuthorityOptions"]).GetAwaiter().GetResult();
            
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = authorityOptions["authorityUrl"];
                    options.ApiName = authorityOptions["apiName"];
                    options.RequireHttpsMetadata = true;
                    options.SupportedTokens = SupportedTokens.Jwt;
                });

            return services;
        }
    }
}