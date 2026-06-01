using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Infrastructure.Http;

/// <summary>
/// Single place that turns a non-success <see cref="HttpResponseMessage"/> into a typed
/// <see cref="UiError"/>. Maps the backend status contract to <see cref="ErrorKind"/> and
/// lifts ProblemDetails fields (Detail, Title, correlationId, field errors).
/// </summary>
public static class ProblemDetailsReader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<UiError> ReadAsync(HttpResponseMessage response, CancellationToken ct = default)
    {
        var kind = MapStatus(response.StatusCode);
        ApiProblemDetails? problem = null;

        // Body may be problem+json, plain json, or empty — never let parsing throw.
        try
        {
            if (response.Content.Headers.ContentLength is not 0)
                problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(JsonOptions, ct);
        }
        catch (JsonException) { /* non-JSON error body — fall back to status-based message */ }

        var correlationId = problem?.CorrelationId
            ?? (response.Headers.TryGetValues("X-Correlation-Id", out var v) ? v.FirstOrDefault() : null);

        var fieldErrors = problem?.Errors is { Count: > 0 }
            ? problem.Errors.ToDictionary(
                kvp => ToCamelCase(kvp.Key),
                kvp => kvp.Value,
                StringComparer.OrdinalIgnoreCase)
            : null;

        var message = problem?.Detail
            ?? problem?.Title
            ?? DefaultMessage(kind);

        return new UiError(kind, message, problem?.Title, correlationId, fieldErrors);
    }

    private static ErrorKind MapStatus(HttpStatusCode status) => status switch
    {
        HttpStatusCode.BadRequest => ErrorKind.Validation,
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => ErrorKind.Unauthorized,
        HttpStatusCode.NotFound => ErrorKind.NotFound,
        HttpStatusCode.Conflict => ErrorKind.Conflict,
        HttpStatusCode.TooManyRequests => ErrorKind.RateLimited,
        _ => ErrorKind.Unexpected
    };

    private static string DefaultMessage(ErrorKind kind) => kind switch
    {
        ErrorKind.Validation => "La información ingresada no es válida.",
        ErrorKind.Unauthorized => "Sesión expirada o sin permisos.",
        ErrorKind.NotFound => "El recurso solicitado no existe.",
        ErrorKind.Conflict => "La operación viola una regla de negocio.",
        ErrorKind.RateLimited => "Demasiadas solicitudes. Reintentá en unos segundos.",
        _ => "Ocurrió un error inesperado en el servidor."
    };

    private static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name) || char.IsLower(name[0]) ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
