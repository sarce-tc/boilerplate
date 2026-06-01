namespace Microservice.Client.Infrastructure.Http;

/// <summary>
/// Scoped holder for the correlation id of the most recent request. The diagnostics
/// panel reads <see cref="LastCorrelationId"/>; handlers and the ProblemDetails reader
/// write to it. Originating the id on the client (not just trusting the server echo)
/// lets us correlate even requests that never reached the API (offline).
/// </summary>
public sealed class CorrelationContext
{
    public string? LastCorrelationId { get; set; }

    /// <summary>New id per logical request. Hex, header-safe — matches the backend's "N" format.</summary>
    public static string NewId() => Guid.NewGuid().ToString("N");
}
