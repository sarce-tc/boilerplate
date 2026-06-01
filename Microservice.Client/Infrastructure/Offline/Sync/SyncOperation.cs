namespace Microservice.Client.Infrastructure.Offline.Sync;

/// <summary>
/// A single queued mutation awaiting delivery. Persisted in IndexedDB so it survives reloads
/// and offline periods. The <see cref="IdempotencyKey"/> is generated once at enqueue time and
/// reused on every replay, so the backend's Idempotency-Key middleware makes retries
/// effectively exactly-once.
/// </summary>
public sealed record SyncOperation
{
    /// <summary>IndexedDB key. Also the idempotency key — one logical mutation, one identity.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>HTTP method as string (POST/PUT/DELETE) — JSON-friendly.</summary>
    public string Method { get; init; } = "POST";

    /// <summary>Relative URL (e.g. "api/v1/sales").</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>Serialized request body, or null for bodiless mutations.</summary>
    public string? JsonBody { get; init; }

    /// <summary>Logical entity type, drives conflict policy and UI grouping (e.g. "sale", "product").</summary>
    public string EntityType { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public int Attempts { get; init; }
    public string? LastError { get; init; }

    /// <summary>The idempotency key sent with every replay (stable for the life of the op).</summary>
    public string IdempotencyKey => Id;
}

/// <summary>Outcome of attempting to deliver one operation.</summary>
public enum SyncOutcome
{
    /// <summary>Delivered (2xx or idempotent replay). Remove from queue.</summary>
    Delivered,
    /// <summary>Transient failure (network/5xx/429). Keep and retry with backoff.</summary>
    Retryable,
    /// <summary>Permanent failure (400/409/404). Move to conflicts — needs human/UI resolution.</summary>
    Rejected
}
