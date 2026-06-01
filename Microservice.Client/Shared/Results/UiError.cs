namespace Microservice.Client.Shared.Results;

/// <summary>
/// Normalized, UI-ready error. Built from a backend ProblemDetails or a transport
/// failure. Carries the <see cref="CorrelationId"/> so it can be shown in diagnostics
/// and matched against backend logs.
/// </summary>
/// <param name="Kind">Classification driving how the UI reacts.</param>
/// <param name="Message">Human-readable summary (ProblemDetails.Detail or a fallback).</param>
/// <param name="Title">Short code/title (ProblemDetails.Title).</param>
/// <param name="CorrelationId">X-Correlation-Id echoed by the API, if any.</param>
/// <param name="FieldErrors">Per-field messages for form binding (validation only).</param>
public sealed record UiError(
    ErrorKind Kind,
    string Message,
    string? Title = null,
    string? CorrelationId = null,
    IReadOnlyDictionary<string, string[]>? FieldErrors = null)
{
    public bool IsValidation => Kind == ErrorKind.Validation;

    public static UiError Network(string? correlationId = null) =>
        new(ErrorKind.Network, "No se pudo conectar con el servidor. Operación encolada o reintentable.", "network_error", correlationId);

    public static UiError Unexpected(string message, string? correlationId = null) =>
        new(ErrorKind.Unexpected, message, "unexpected_error", correlationId);
}
