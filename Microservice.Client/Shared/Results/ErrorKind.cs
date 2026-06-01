namespace Microservice.Client.Shared.Results;

/// <summary>
/// UI-facing classification of a failed operation. Maps 1:1 from the backend's
/// HTTP status contract (see GlobalExceptionHandler / ProblemDetails) so every
/// feature reacts to failures the same way.
/// </summary>
public enum ErrorKind
{
    /// <summary>400 — input validation (FluentValidation failures from the server).</summary>
    Validation,
    /// <summary>401/403 — not authenticated or not authorized.</summary>
    Unauthorized,
    /// <summary>404 — resource not found.</summary>
    NotFound,
    /// <summary>409 — business rule / DomainException.</summary>
    Conflict,
    /// <summary>429 — rate limited; caller may retry after backoff.</summary>
    RateLimited,
    /// <summary>Transport failure / offline — request never reached the server.</summary>
    Network,
    /// <summary>5xx or anything unmapped.</summary>
    Unexpected
}
