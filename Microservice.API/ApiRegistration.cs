using Microsoft.AspNetCore.Mvc;
using Microservice.API.Extensions;
using Microsoft.OpenApi;
using Asp.Versioning;
using Asp.Versioning.Builder;
using System.Reflection;

namespace Microservice.API
{
    /// <summary>
    /// API Services Registration
    /// 
    /// Configures:
    /// - Controllers
    /// - Swagger/OpenAPI
    /// - Global Exception Handling
    /// - Logging
    /// - Configuration
    /// </summary>
    public static class ApiRegistration
    {
        public static IServiceCollection AddApiServices(
            this IServiceCollection services,
            IConfigurationManager configuration,
            IWebHostEnvironment hostEnvironment)
        {
            // Add services to the container
            services.AddControllers();

            // Configure API Versioning - Simple approach
            var versioningBuilder = services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),           // /api/v1/examples
                    new QueryStringApiVersionReader("api-version"), // ?api-version=1.0
                    new HeaderApiVersionReader("X-Version")        // X-Version: 1.0
                );
            });

            // Add API Explorer for versioned documentation
            versioningBuilder.AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Register global exception handler (Modern .NET 10 approach)
            services.AddGlobalExceptionHandler();

            // Configure Swagger/OpenAPI for multiple versions
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                // Version 1 - Current stable version
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Microservice API",
                    Version = "v1.0",
                    Description = "Microservice API with CQRS pattern and Result error handling - Version 1.0 (Stable)",
                    Contact = new OpenApiContact
                    {
                        Name = "API Team",
                        Email = "api@example.com"
                    }
                });

                // Version 2 - Enhanced version
                c.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = "Microservice API",
                    Version = "v2.0",
                    Description = "Microservice API with enhanced features - Version 2.0 (Preview)",
                    Contact = new OpenApiContact
                    {
                        Name = "API Team",
                        Email = "api@example.com"
                    }
                });

                // Include XML comments if they exist
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            configuration
                .AddJsonFile($"appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.json", optional: true);

            configuration.AddEnvironmentVariables();

            //if (hostEnvironment.IsDevelopment() && File.Exists("../.env.local"))
            //    configuration.AddDotNetEnv("../.env.local");

            services.AddObservability(configuration, hostEnvironment);
            services.AddCustomHealthChecks(configuration);
            services.AddCustomRateLimiting(configuration);

            services.AddCors(options =>
            {
                options.AddPolicy("DefaultCors", policy =>
                {
                    if (hostEnvironment.IsDevelopment() || hostEnvironment.EnvironmentName == "Testing")
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                    else
                    {
                        policy.WithOrigins("http://")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                });
            });

            return services;
        }
    }
}