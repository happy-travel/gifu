using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Services;
using HappyTravel.Gifu.Api.Services.CurrencyConverter;
using HappyTravel.Gifu.Api.Services.SupplierClients;
using HappyTravel.Gifu.Api.Services.VccServices;
using HappyTravel.Gifu.Data;
using HappyTravel.Gifu.Data.CompiledModels;
using HappyTravel.HttpRequestLogger;
using HappyTravel.Money.Enums;
using HappyTravel.VaultClient;
using IdentityModel.Client;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions;

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


    public static IServiceCollection ConfigureVccServices(this IServiceCollection services, IVaultClient vaultClient, IConfiguration configuration)
    {
        var amExAccounts = vaultClient.Get(configuration["AmexAccounts"])
                .GetAwaiter().GetResult();
        var ixarisAccounts = vaultClient.Get(configuration["IxarisAccounts"])
                .GetAwaiter().GetResult();

        ConfigureAmExIssuer();
        ConfigureIxarisIssuer();
        ConfigureVccService();
        ConfigureVccServiceResolver();

        return services;               


        void ConfigureAmExIssuer()
        {
            if (configuration.GetValue<bool>("Testing:UseFakeAmexClient"))
            {
                services.AddTransient<IAmExClient, FakeAmexClient>();
            }
            else
            {
                services.AddHttpClient<IAmExClient, AmExClient>()
                    .AddHttpClientRequestLogging(configuration);
            }

            var amExOptions = vaultClient.Get(configuration["AmExOptions"])
                .GetAwaiter().GetResult();
           
            var accounts = amExAccounts.Select(a => new
            {
                Currency = Enum.Parse<Currencies>(a.Key),
                AccountId = a.Value
            })
                .ToDictionary(a => a.Currency, a => a.AccountId);

            services.Configure<AmExOptions>(o =>
            {
                o.Endpoint = amExOptions["endpoint"];
                o.ClientId = amExOptions["clientId"];
                o.ClientSecret = amExOptions["clientSecret"];
                o.Accounts = accounts;
            })
                .AddTransient<IVccSupplierService, AmExService>();
        }


        void ConfigureIxarisIssuer()
        {
            var ixarisOptions = vaultClient.Get(configuration["IxarisOptions"])
                .GetAwaiter().GetResult();

            services.AddHttpClient(HttpClientNames.IxarisClient, client =>
            {
                client.BaseAddress = new Uri(ixarisOptions["endPoint"]);
            })
            .AddHttpClientRequestLogging(configuration);
            
            var accounts = ixarisAccounts.Select(o => new
            {
                Currency = Enum.Parse<Currencies>(o.Key),
                AccountId = o.Value
            })
                .ToDictionary(a => a.Currency, a => a.AccountId);

            var ixarisVccFactoryNames = vaultClient.Get(configuration["IxarisVccFactoryNames"])
                .GetAwaiter().GetResult();

            var vccFactoryNames = ixarisVccFactoryNames.Select(a => new
            {
                CreditCardType = Enum.Parse<CreditCardTypes>(a.Key),
                VccFactoryName = a.Value
            })
                .ToDictionary(a => a.CreditCardType, a => a.VccFactoryName);

            services.Configure<IxarisOptions>(o =>
            {
                o.ApiKey = ixarisOptions["apiKey"];
                o.Password = ixarisOptions["password"];
                o.Accounts = accounts;
                o.VccFactoryNames = vccFactoryNames;
                o.DefaultVccType = CreditCardTypes.Visa;
            })
                .AddScoped<IVccSupplierService, IxarisService>()
                .AddTransient<IIxarisClient, IxarisClient>();
        }


        void ConfigureVccService()
        {
            services.Configure<VccServiceOptions>(configuration.GetSection("VccServiceOptions"));
        }


        void ConfigureVccServiceResolver()
        {            
            var amExCurrencies = amExAccounts.Select(a => Enum.Parse<Currencies>(a.Key)).ToList();            
            var ixarisCurrencies = ixarisAccounts.Select(a => Enum.Parse<Currencies>(a.Key)).ToList();

            services.Configure<VccServiceResolverOptions>(o =>
            {
                o.AmexCurrencies = amExCurrencies;
                o.AmexCreditCardTypes = new() { CreditCardTypes.AmericanExpress };
                o.IxarisCurrencies = ixarisCurrencies;
                o.IxarisCreditCardTypes = new() { CreditCardTypes.Visa, CreditCardTypes.MasterCard };
            });
        }
    }


    public static IServiceCollection ConfigureCurrencyConverterService(this IServiceCollection services, IVaultClient vaultClient, IConfiguration configuration)
    {
        var authorityOptions = configuration.GetSection("Authority").Get<AuthorityOptions>();
        var clientOptions = vaultClient.Get(configuration["IdentityClient:Options"]).GetAwaiter().GetResult();
        var identityUri = new Uri(new Uri(authorityOptions.AuthorityUrl), "/connect/token").ToString();
        var clientId = clientOptions["clientId"];
        var clientSecret = clientOptions["clientSecret"];

        services.AddAccessTokenManagement(options =>
        {
            options.Client.Clients.Add(HttpClientNames.AccessTokenClient, new ClientCredentialsTokenRequest
            {
                Address = identityUri,
                ClientId = clientId,
                ClientSecret = clientSecret,
            });
        });

        var currencyConverterOptions = vaultClient.Get(configuration["CurrencyConverter"]).GetAwaiter().GetResult();

        services.AddClientAccessTokenHttpClient(HttpClientNames.CurrencyConverterClient, HttpClientNames.AccessTokenClient, client =>
        {
            client.BaseAddress = new Uri(currencyConverterOptions["endPoint"]);
        })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetDefaultRetryPolicy());

        return services.AddTransient<CurrencyConverterClient>()
            .AddTransient<CurrencyConverterService>()
            .AddTransient<CurrencyConverterStorage>();
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
                builder.EnableRetryOnFailure(3);
            });
            options.UseInternalServiceProvider(null);
            options.EnableSensitiveDataLogging(false);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            // Need to run `dotnet ef dbcontext optimize` in HappyTravel.Gifu.Data project after changing for regenerating compiled models
            options.UseModel(GifuContextModel.Instance);
        }, 16);
    }
        
        
    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authorityOptions = configuration.GetSection("Authority").Get<AuthorityOptions>();

        services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authorityOptions.AuthorityUrl;
                options.Audience = authorityOptions.Audience;
                options.RequireHttpsMetadata = true;
                options.AutomaticRefreshInterval = authorityOptions.AutomaticRefreshInterval;
            });

        return services;
    }


    private static IAsyncPolicy<HttpResponseMessage> GetDefaultRetryPolicy()
    {
        var jitter = new Random();

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.ServiceUnavailable || msg.StatusCode == HttpStatusCode.Unauthorized)
            .WaitAndRetryAsync(3, attempt
                => TimeSpan.FromSeconds(Math.Pow(1.5, attempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 100)));
    }
}