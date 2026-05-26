using Microservice.Application.Common.Results;
using Microservice.Application.Exceptions;
using Microservice.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Microservice.API.ExceptionHandling
{
    // ═══════════════════════════════════════════════════════════════════════
    // AGENT — Sole exception handler for the entire pipeline.
    // Do NOT add try-catch in handlers or services (except UoW RollbackAsync).
    //
    // Exception → HTTP mapping:
    //   DomainException            → 409 Conflict   (Warning log)
    //   ValidationException        → 400 Bad Request (Warning log)
    //   ArgumentException          → 400 Bad Request (Warning log)
    //   KeyNotFoundException       → 404 Not Found   (Info log)
    //   InvalidOperationException  → 409 Conflict    (Error log)
    //   UnauthorizedAccessException→ 401 Unauthorized(Error log)
    //   NotImplementedException    → 501             (Error log)
    //   Everything else            → 500             (Error log)
    //
    // Response format: RFC 7807 ProblemDetails (application/problem+json)
    // Extensions: traceId · correlationId · timestamp · exceptionType (dev only)
    // ═══════════════════════════════════════════════════════════════════════
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var traceId       = Activity.Current?.Id ?? httpContext.TraceIdentifier;
            var correlationId = httpContext.Items["X-Correlation-Id"]?.ToString();
            var instance      = httpContext.Request.Path;

            LogException(exception, traceId, correlationId);

            var (error, statusCode) = MapExceptionToError(exception);

            var problemDetails = CreateProblemDetails(
                error, statusCode, exception, traceId, correlationId, instance);

            httpContext.Response.StatusCode = statusCode;

            // Pass contentType explicitly — WriteAsJsonAsync would override it to
            // "application/json; charset=utf-8" without the explicit parameter.
            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                options: null,
                contentType: "application/problem+json",
                cancellationToken: cancellationToken);

            return true;
        }

        private static (Error error, int statusCode) MapExceptionToError(Exception exception) => exception switch
        {
            ValidationException validationEx => (
                CreateValidationError(validationEx),
                StatusCodes.Status400BadRequest),

            KeyNotFoundException => (
                Error.NotFound("The requested resource was not found"),
                StatusCodes.Status404NotFound),

            DomainException => (
                Error.Conflict(exception.Message),
                StatusCodes.Status409Conflict),

            InvalidOperationException => (
                Error.Conflict(exception.Message),
                StatusCodes.Status409Conflict),

            UnauthorizedAccessException => (
                Error.Unauthorized("Access denied"),
                StatusCodes.Status401Unauthorized),

            NotImplementedException => (
                Error.Generic("This feature is not yet implemented"),
                StatusCodes.Status501NotImplemented),

            ArgumentException => (
                Error.Validation(exception.Message),
                StatusCodes.Status400BadRequest),

            _ => (
                Error.Generic($"An unexpected error occurred: {exception.GetType().Name}"),
                StatusCodes.Status500InternalServerError)
        };

        private static Error CreateValidationError(ValidationException ex)
        {
            if (ex.Failures is null || ex.Failures.Count == 0)
                return Error.Validation("Validation failed");

            var message = string.Join("; ", ex.Failures.Take(3)
                .Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));

            if (ex.Failures.Count > 3)
                message += $"; and {ex.Failures.Count - 3} more errors";

            return Error.Validation(message);
        }

        private static ProblemDetails CreateProblemDetails(
            Error error, int statusCode, Exception exception,
            string traceId, string? correlationId, string instance)
        {
            var pd = new ProblemDetails
            {
                Type     = $"https://httpstatuses.com/{statusCode}",
                Title    = error.Code,
                Detail   = error.Message,
                Status   = statusCode,
                Instance = instance
            };

            pd.Extensions["traceId"]       = traceId;
            pd.Extensions["correlationId"] = correlationId;
            pd.Extensions["timestamp"]     = DateTime.UtcNow;

            if (IsDevelopment())
            {
                pd.Extensions["exceptionType"]    = exception.GetType().Name;
                pd.Extensions["exceptionMessage"] = exception.Message;
                if (exception.InnerException is not null)
                    pd.Extensions["innerException"] = exception.InnerException.Message;
            }

            return pd;
        }

        private void LogException(Exception exception, string traceId, string? correlationId)
        {
            var logLevel = exception switch
            {
                ValidationException or DomainException or ArgumentException => LogLevel.Warning,
                KeyNotFoundException                                         => LogLevel.Information,
                _                                                            => LogLevel.Error
            };

            logger.Log(logLevel, exception,
                "Unhandled exception. TraceId: {TraceId}, CorrelationId: {CorrelationId}, Type: {ExceptionType}",
                traceId, correlationId, exception.GetType().Name);
        }

        private static bool IsDevelopment() =>
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    }
}
