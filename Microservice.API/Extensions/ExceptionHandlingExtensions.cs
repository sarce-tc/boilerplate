using Microservice.API.ExceptionHandling;

namespace Microservice.API.Extensions
{
    /// <summary>
    /// Extension methods for registering exception handling
    /// 
    /// Modern .NET 10 approach using IExceptionHandler
    /// 
    /// Usage in Program.cs:
    /// builder.Services.AddGlobalExceptionHandler();
    /// app.UseExceptionHandler(_ => { });
    /// </summary>
    public static class ExceptionHandlingExtensions
    {
        /// <summary>
        /// Register global exception handler
        /// 
        /// This registers the GlobalExceptionHandler as an IExceptionHandler
        /// which is the modern way to handle exceptions in .NET 10+
        /// 
        /// Must be paired with:
        /// app.UseExceptionHandler(_ => { });
        /// </summary>
        public static IServiceCollection AddGlobalExceptionHandler(
            this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            return services;
        }
    }
}
