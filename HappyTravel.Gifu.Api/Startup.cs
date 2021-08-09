using System;
using System.Collections.Generic;
using HappyTravel.ErrorHandling.Extensions;
using HappyTravel.Gifu.Api.Infrastructure.Environment;
using HappyTravel.Gifu.Api.Infrastructure.Extensions;
using HappyTravel.Gifu.Api.Services;
using HappyTravel.Gifu.Data;
using HappyTravel.StdOutLogger.Extensions;
using HappyTravel.Telemetry.Extensions;
using HappyTravel.VaultClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Gifu.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            using var vaultClient = new VaultClient.VaultClient(new VaultOptions
            {
                BaseUrl = new Uri(EnvironmentVariableHelper.Get("Vault:Endpoint", Configuration)),
                Engine = Configuration["Vault:Engine"],
                Role = Configuration["Vault:Role"]
            });
            
            vaultClient.Login(EnvironmentVariableHelper.Get("Vault:Token", Configuration)).GetAwaiter().GetResult();

            services
                .AddMvcCore()
                .AddAuthorization(options =>
                {
                    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                    options.AddPolicy("CanCreateCard", policy =>
                    {
                        policy.RequireClaim("scope", "vcc.create_card");
                    });
                    options.AddPolicy("CanGetReport", policy =>
                    {
                        policy.RequireClaim("scope", "vcc.report");
                    });
                })
                .AddApiExplorer();

            services.AddHealthChecks()
                .AddDbContextCheck<GifuContext>();

            services
                .AddProblemDetailsErrorHandling()
                .AddHttpContextAccessor()
                .ConfigureApiVersioning()
                .ConfigureSwagger()
                .ConfigureDatabaseOptions(vaultClient, Configuration)
                .ConfigureAuthentication(vaultClient, Configuration)
                .ConfigureIssuer(vaultClient, Configuration)
                .AddTransient<IClientService, ClientService>()
                .AddTracing(Configuration, options =>
                {
                    options.ServiceName = $"{HostEnvironment.ApplicationName}-{HostEnvironment.EnvironmentName}";
                    options.JaegerHost = HostEnvironment.IsLocal()
                        ? Configuration.GetValue<string>("Jaeger:AgentHost")
                        : Configuration.GetValue<string>(Configuration.GetValue<string>("Jaeger:AgentHost"));
                    options.JaegerPort = HostEnvironment.IsLocal()
                        ? Configuration.GetValue<int>("Jaeger:AgentPort")
                        : Configuration.GetValue<int>(Configuration.GetValue<string>("Jaeger:AgentPort"));
                });
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<Startup>();
            app.UseProblemDetailsErrorHandler(env, logger);
            app.UseHttpContextLogging(options => options.IgnoredPaths = new HashSet<string> {"/health"});
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HappyTravel.Gifu.Api v1"));
            }

            app
                .UseHttpsRedirection()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapHealthChecks("/health");
                    endpoints.MapControllers();
                });
        }
        
        
        private IConfiguration Configuration { get; }
        private IHostEnvironment HostEnvironment { get; }
    }
}
