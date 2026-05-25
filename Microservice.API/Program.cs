using Microservice.API;
using Microservice.API.Extensions;
using Microservice.API.Middleware;
using Microservice.Application;
using Microservice.Infrastructure;
using Serilog;

// Bootstrap logger captures startup errors before host is built
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {SourceContext} {Message:lj}{NewLine}{Exception}"));

    builder.Services.AddApiServices(builder.Configuration, builder.Environment);
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    var app = builder.Build();

    // CorrelationId must be first: it pushes the ID to Serilog LogContext so every
    // downstream middleware — including the exception handler — logs with it.
    app.UseCorrelationId();
    app.UseExceptionHandler(_ => { });
    app.UseSerilogRequestLogging();
    app.UseRateLimiter();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1.0 (Stable)");
            c.SwaggerEndpoint("/swagger/v2/swagger.json", "API v2.0 (Preview)");
            c.RoutePrefix = string.Empty;
            c.DisplayRequestDuration();
            c.ConfigObject.AdditionalItems["showExtensions"] = true;
            c.ConfigObject.AdditionalItems["showCommonExtensions"] = true;
        });
    }

    app.UseCors("DefaultCors");
    app.UseIdempotency();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.MapCustomHealthChecks();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
