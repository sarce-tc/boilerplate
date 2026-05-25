using Microservice.Application.Common.Results;
using Microservice.Application.Exceptions;
using Microservice.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Microservice.API.ExceptionHandling
{
    /// <summary>
    /// Global Exception Handler using IExceptionHandler (Modern .NET 10 approach)
    /// 
    /// Purpose: Centralized exception handling for all unhandled exceptions
    /// 
    /// Features:
    /// - Handles all exception types uniformly
    /// - Maps exceptions to appropriate HTTP status codes
    /// - Returns RFC 7807 ProblemDetails format
    /// - Integrates with structured Error system
    /// - Includes trace IDs for debugging
    /// - Supports custom exception mapping
    /// 
    /// Exception Mapping:
    /// - ValidationException → 400 Bad Request
    /// - KeyNotFoundException → 404 Not Found
    /// - InvalidOperationException → 409 Conflict
    /// - UnauthorizedAccessException → 401 Unauthorized
    /// - NotImplementedException → 501 Not Implemented
    /// - ArgumentException → 400 Bad Request
    /// - Generic Exception → 500 Internal Server Error
    /// 
    /// Modern Features:
    /// - Primary Constructor (C# 14)
    /// - Record-style patterns
    /// - Async exception handling
    /// - Structured logging support
    /// - Trace context preservation
    /// 
    /// Usage:
    /// 1. Register in Program.cs:
    ///    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    ///    app.UseExceptionHandler(_ => { });
    /// 
    /// 2. Or use extension method:
    ///    builder.Services.AddGlobalExceptionHandler();
    /// </summary>
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        /// <summary>
        /// Handle exceptions globally
        /// 
        /// Process:
        /// 1. Extract exception details
        /// 2. Log the exception
        /// 3. Map to appropriate HTTP status
        /// 4. Create ProblemDetails response
        /// 5. Write response
        /// </summary>
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
            var correlationId = httpContext.Items["X-Correlation-Id"]?.ToString();
            var instance = httpContext.Request.Path;

            LogException(exception, traceId, correlationId);

            // Map exception to error
            var (error, statusCode) = MapExceptionToError(exception);

            // Create ProblemDetails response
            var problemDetails = CreateProblemDetails(
                error,
                statusCode,
                exception,
                traceId,
                correlationId,
                instance
            );

            // Set response
            httpContext.Response.StatusCode = statusCode;

            // Write response — pass contentType explicitly so WriteAsJsonAsync
            // does not override it to "application/json; charset=utf-8"
            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                options: null,
                contentType: "application/problem+json",
                cancellationToken: cancellationToken);

            return true;
        }

        /// <summary>
        /// Map exception type to Error and HTTP status code
        /// 
        /// Handles:
        /// - Custom exceptions (ValidationException, etc.)
        /// - Standard .NET exceptions
        /// - Unknown exceptions (500 default)
        /// </summary>
        private static (Error error, int statusCode) MapExceptionToError(Exception exception) => exception switch
        {
            // Validation Exception
            ValidationException validationEx => (
                CreateValidationError(validationEx),
                StatusCodes.Status400BadRequest
            ),

            // Not Found
            KeyNotFoundException => (
                Error.NotFound("The requested resource was not found"),
                StatusCodes.Status404NotFound
            ),

            // Domain rule violation — known business invariant rejected (e.g. cancel a completed order)
            DomainException => (
                Error.Conflict(exception.Message),
                StatusCodes.Status409Conflict
            ),

            // Conflict/State
            InvalidOperationException => (
                Error.Conflict(exception.Message),
                StatusCodes.Status409Conflict
            ),

            // Unauthorized
            UnauthorizedAccessException => (
                Error.Unauthorized("Access denied"),
                StatusCodes.Status401Unauthorized
            ),

            // Not Implemented
            NotImplementedException => (
                Error.Generic("This feature is not yet implemented"),
                StatusCodes.Status501NotImplemented
            ),

            // Argument/Validation
            ArgumentException => (
                Error.Validation(exception.Message),
                StatusCodes.Status400BadRequest
            ),

            // Generic error - Internal Server Error
            _ => (
                Error.Generic($"An unexpected error occurred: {exception.GetType().Name}"),
                StatusCodes.Status500InternalServerError
            )
        };

        /// <summary>
        /// Create validation error from ValidationException
        /// 
        /// Extracts all validation failures and creates Error objects
        /// </summary>
        private static Error CreateValidationError(ValidationException ex)
        {
            if (ex.Failures == null || ex.Failures.Count == 0)
                return Error.Validation("Validation failed");

            // Combine first few failures into message
            var messages = ex.Failures
                .Take(3)
                .Select(f => $"{f.PropertyName}: {f.ErrorMessage}")
                .ToList();

            var message = string.Join("; ", messages);
            if (ex.Failures.Count > 3)
                message += $"; and {ex.Failures.Count - 3} more errors";

            return Error.Validation(message);
        }

        /// <summary>
        /// Create RFC 7807 ProblemDetails response
        /// 
        /// Includes:
        /// - Error code and message
        /// - HTTP status code
        /// - Trace ID for debugging
        /// - Type URI for classification
        /// - Timestamp
        /// - Exception details (development only)
        /// </summary>
        private static ProblemDetails CreateProblemDetails(
            Error error,
            int statusCode,
            Exception exception,
            string traceId,
            string? correlationId,
            string instance)
        {
            var problemDetails = new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{statusCode}",
                Title = error.Code,
                Detail = error.Message,
                Status = statusCode,
                Instance = instance
            };

            problemDetails.Extensions["traceId"] = traceId;
            problemDetails.Extensions["correlationId"] = correlationId;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

            // Add exception details in development
            if (IsDevelopment())
            {
                problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
                problemDetails.Extensions["exceptionMessage"] = exception.Message;
                
                if (exception.InnerException != null)
                {
                    problemDetails.Extensions["innerException"] = exception.InnerException.Message;
                }
            }

            return problemDetails;
        }

        /// <summary>
        /// Log exception with structured information
        /// 
        /// Logs:
        /// - Exception type
        /// - Message
        /// - Stack trace (on error level)
        /// - Trace ID
        /// </summary>
        private void LogException(Exception exception, string traceId, string? correlationId)
        {
            var logLevel = exception switch
            {
                ValidationException  => LogLevel.Warning,
                DomainException      => LogLevel.Warning,   // expected business-rule rejection
                KeyNotFoundException => LogLevel.Information,
                _ => LogLevel.Error
            };

            logger.Log(
                logLevel,
                exception,
                "Unhandled exception. TraceId: {TraceId}, CorrelationId: {CorrelationId}, ExceptionType: {ExceptionType}",
                traceId,
                correlationId,
                exception.GetType().Name
            );
        }

        /// <summary>
        /// Check if running in Development environment
        /// </summary>
        private static bool IsDevelopment()
        {
            // Note: Inject IHostEnvironment if needed
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        }
    }
}
