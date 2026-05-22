using Microservice.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Extensions
{
    /// <summary>
    /// Extension methods for converting Result<T> to IActionResult
    /// 
    /// Use Case: Simplify controller response handling with proper HTTP status codes
    /// 
    /// Pattern: Convert result pattern to RFC 7807 Problem Details
    /// 
    /// Success Cases:
    /// - With data: Returns 200 OK with data
    /// - Without data (null): Returns 204 No Content
    /// 
    /// Failure Cases:
    /// - Single error: Returns appropriate status code with error details
    /// - Multiple errors: Returns 400 with all errors in extensions
    /// - Status code mapped from error code
    /// 
    /// Benefits:
    /// - Reduces boilerplate in controllers
    /// - Consistent error response format (RFC 7807)
    /// - Automatic status code mapping
    /// - Supports multiple errors
    /// - Type-safe implementation
    /// 
    /// Example Usage:
    /// var result = await mediator.Send(command);
    /// return result.ToActionResult();  // ✅ Clean
    /// 
    /// or with custom success status:
    /// return result.ToActionResult(StatusCodes.Status201Created);
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Convert Result to IActionResult
        /// 
        /// Success: 204 No Content
        /// Failure: Mapped status code with error details
        /// </summary>
        public static IActionResult ToActionResult(this Result result)
        {
            if (result.IsSuccess)
                return new NoContentResult();

            return CreateProblem(result.Errors);
        }

        /// <summary>
        /// Convert Result<T> to IActionResult
        /// 
        /// Success with data: 200 OK
        /// Success without data: 204 No Content
        /// Failure: Mapped status code with error details
        /// </summary>
        public static IActionResult ToActionResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
                return result.Value is null
                    ? new NoContentResult()
                    : new OkObjectResult(result.Value);

            return CreateProblem(result.Errors);
        }

        /// <summary>
        /// Convert Result<T> to IActionResult with custom success status code
        /// 
        /// Useful for:
        /// - POST requests (201 Created)
        /// - Specific success status codes
        /// 
        /// Example:
        /// var result = await mediator.Send(createCommand);
        /// return result.ToActionResult(StatusCodes.Status201Created);
        /// </summary>
        public static IActionResult ToActionResult<T>(
            this Result<T> result,
            int successStatusCode)
        {
            if (result.IsSuccess)
                return result.Value is null
                    ? new NoContentResult()
                    : new ObjectResult(result.Value) { StatusCode = successStatusCode };

            return CreateProblem(result.Errors);
        }

        /// <summary>
        /// Create RFC 7807 ProblemDetails from errors
        /// 
        /// Features:
        /// - Uses primary error for title and status
        /// - Includes error type URL
        /// - Attaches additional errors if multiple exist
        /// </summary>
        private static IActionResult CreateProblem(List<Error> errors)
        {
            if (!errors.Any())
                return new ObjectResult(new ProblemDetails
                {
                    Title = "Unknown error",
                    Detail = "An unexpected error occurred",
                    Status = StatusCodes.Status400BadRequest
                })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };

            var primaryError = errors.First();
            var statusCode = MapStatusCode(primaryError.Code);

            var problemDetails = new ProblemDetails
            {
                Title = primaryError.Code,
                Detail = primaryError.Message,
                Status = statusCode,
                Type = $"https://httpstatuses.com/{statusCode}"
            };

            // Attach multiple errors if exist
            if (errors.Count > 1)
            {
                problemDetails.Extensions["errors"] = errors
                    .Select(e => new { e.Code, e.Message })
                    .ToList();
            }

            return new ObjectResult(problemDetails)
            {
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Map error code to HTTP status code
        /// 
        /// Mapping:
        /// - "Validation" → 400 Bad Request
        /// - "NotFound" → 404 Not Found
        /// - "Conflict" → 409 Conflict
        /// - "Unauthorized" → 401 Unauthorized
        /// - "Forbidden" → 403 Forbidden
        /// - Default → 400 Bad Request
        /// </summary>
        private static int MapStatusCode(string errorCode) =>
            errorCode switch
            {
                "Validation" => StatusCodes.Status400BadRequest,
                "NotFound" => StatusCodes.Status404NotFound,
                "Conflict" => StatusCodes.Status409Conflict,
                "Unauthorized" => StatusCodes.Status401Unauthorized,
                "Forbidden" => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };
    }
}
