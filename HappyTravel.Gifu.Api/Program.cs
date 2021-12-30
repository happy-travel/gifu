using System.Collections.Generic;
using HappyTravel.ErrorHandling.Extensions;
using HappyTravel.Gifu.Api.Infrastructure.Extensions;
using HappyTravel.StdOutLogger.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureAppConfiguration();
builder.ConfigureLogging();
builder.ConfigureSentry();
builder.ConfigureServiceProvider();
builder.ConfigureServices();

var app = builder.Build();

app.UseProblemDetailsErrorHandler(app.Environment, app.Logger);
app.UseHttpContextLogging(options => options.IgnoredPaths = new HashSet<string> {"/health"});
            
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HappyTravel.Gifu.Api v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health").AllowAnonymous();
    endpoints.MapControllers();
});

app.Run();