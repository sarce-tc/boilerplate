namespace Microservice.Client.Infrastructure.Resilience;

/// <summary>
/// Exponential backoff with full jitter. Used by the SyncProcessor between replay attempts.
/// Kept dependency-free (no Polly) — the policy surface we need is one method.
/// </summary>
public static class RetryPolicy
{
    /// <summary>Delay before attempt N (1-based). Caps at <paramref name="maxDelay"/>.</summary>
    public static TimeSpan Backoff(int attempt, TimeSpan? baseDelay = null, TimeSpan? maxDelay = null)
    {
        var @base = baseDelay ?? TimeSpan.FromSeconds(1);
        var cap = maxDelay ?? TimeSpan.FromMinutes(2);

        var exp = Math.Min(cap.TotalMilliseconds, @base.TotalMilliseconds * Math.Pow(2, attempt - 1));
        var jittered = Random.Shared.NextDouble() * exp; // full jitter
        return TimeSpan.FromMilliseconds(jittered);
    }
}
