namespace Microservice.Client.Infrastructure.Http;

/// <summary>
/// Strongly-typed keys for per-request metadata flowed through HttpRequestMessage.Options
/// into the DelegatingHandler pipeline. Keeps cross-cutting concerns out of gateway code.
/// </summary>
public static class RequestOptions
{
    /// <summary>
    /// Stable idempotency key for a mutation. Set by the gateway/SyncQueue so that retries
    /// of the SAME logical command reuse the SAME key (backend caches it for 24h → exactly-once).
    /// </summary>
    public static readonly HttpRequestOptionsKey<string> IdempotencyKey = new("Idempotency-Key");

    /// <summary>When set, suppresses the AuthTokenHandler (e.g. the /auth/token call itself).</summary>
    public static readonly HttpRequestOptionsKey<bool> Anonymous = new("Anonymous");
}
