using Microservice.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Extensions
{
    // ═══════════════════════════════════════════════════════════════════════
    // AGENT — Converts Result / Result<T> to IActionResult in controllers.
    //
    //   result.ToActionResult()                     → 204 (no data) or error
    //   result.ToActionResult<T>()                  → 200 OK / 204 / error
    //   result.ToActionResult(Status201Created)     → 201 Created / error
    //
    //   Error code → HTTP status mapping (see MapStatusCode below):
    //     Validation → 400 · NotFound → 404 · Conflict → 409
    //     Unauthorized → 401 · Forbidden → 403 · _ → 400
    // ═══════════════════════════════════════════════════════════════════════
    public static class ResultExtensions
    {
        // Result (no value) → 204 on success
        public static IActionResult ToActionResult(this Result result)
        {
            if (result.IsSuccess)
                return new NoContentResult();

            return CreateProblem(result.Errors);
        }

        // Result<T> → 200 OK with value, 204 if value is null
        public static IActionResult ToActionResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
                return result.Value is null
                    ? new NoContentResult()
                    : new OkObjectResult(result.Value);

            return CreateProblem(result.Errors);
        }

        // Result<T> → custom success status (e.g. 201 Created for POST)
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
