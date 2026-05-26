namespace Microservice.Application.Common.Results;

// ═══════════════════════════════════════════════════════════════════════════
// AGENT — Error codes and their HTTP mapping (via GlobalExceptionHandler / ResultExtensions)
//
//   Error.Validation(msg)   → 400 Bad Request
//   Error.NotFound(msg)     → 404 Not Found
//   Error.Conflict(msg)     → 409 Conflict
//   Error.Unauthorized(msg) → 401 Unauthorized
//   Error.Forbidden(msg)    → 403 Forbidden
//   Error.Generic(msg)      → 500 Internal Server Error
// ═══════════════════════════════════════════════════════════════════════════

public class Error
{
    public string Code    { get; }
    public string Message { get; }

    public Error(string code, string message) { Code = code; Message = message; }

    public static Error Validation(string message)   => new("Validation",   message);
    public static Error NotFound(string message)     => new("NotFound",     message);
    public static Error Conflict(string message)     => new("Conflict",     message);
    public static Error Unauthorized(string message) => new("Unauthorized", message);
    public static Error Forbidden(string message)    => new("Forbidden",    message);
    public static Error Generic(string message)      => new("Generic",      message);
}
