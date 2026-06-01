using System.Text.Json.Serialization;

namespace Microservice.Client.Shared.Contracts;

/// <summary>
/// Client-side mirror of the backend RFC 7807 payload (application/problem+json)
/// produced by GlobalExceptionHandler. Only the fields the UI consumes are mapped;
/// unknown extensions are tolerated.
/// </summary>
public sealed class ApiProblemDetails
{
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("status")] public int? Status { get; set; }
    [JsonPropertyName("detail")] public string? Detail { get; set; }
    [JsonPropertyName("instance")] public string? Instance { get; set; }

    // Extensions emitted by the backend handler.
    [JsonPropertyName("traceId")] public string? TraceId { get; set; }
    [JsonPropertyName("correlationId")] public string? CorrelationId { get; set; }
    [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// Per-field validation errors. Present when the backend returns a
    /// ValidationProblemDetails-shaped body ({ "errors": { field: [msgs] } }).
    /// </summary>
    [JsonPropertyName("errors")] public Dictionary<string, string[]>? Errors { get; set; }
}
