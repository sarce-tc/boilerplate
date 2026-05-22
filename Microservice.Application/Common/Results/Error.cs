namespace Microservice.Application.Common.Results;

/// <summary>
/// Represents an error with code and message
/// 
/// Use Case: Structured error information for Result pattern
/// 
/// Properties:
/// - Code: Error code for mapping to HTTP status codes
/// - Message: Human-readable error message
/// 
/// Error Codes (mapped to HTTP status):
/// - "Validation" → 400 Bad Request
/// - "NotFound" → 404 Not Found
/// - "Conflict" → 409 Conflict
/// - "Unauthorized" → 401 Unauthorized
/// - "Forbidden" → 403 Forbidden
/// - Default → 400 Bad Request
/// </summary>
public class Error
{
    /// <summary>
    /// Error code for categorization and HTTP mapping
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string Message { get; }

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// Validation error (400 Bad Request)
    /// </summary>
    public static Error Validation(string message) => new("Validation", message);

    /// <summary>
    /// Not found error (404 Not Found)
    /// </summary>
    public static Error NotFound(string message) => new("NotFound", message);

    /// <summary>
    /// Conflict error (409 Conflict)
    /// </summary>
    public static Error Conflict(string message) => new("Conflict", message);

    /// <summary>
    /// Unauthorized error (401 Unauthorized)
    /// </summary>
    public static Error Unauthorized(string message) => new("Unauthorized", message);

    /// <summary>
    /// Forbidden error (403 Forbidden)
    /// </summary>
    public static Error Forbidden(string message) => new("Forbidden", message);

    /// <summary>
    /// Generic error (400 Bad Request)
    /// </summary>
    public static Error Generic(string message) => new("Generic", message);
}
