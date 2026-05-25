namespace Microservice.Application.Models.Jobs;

/// <summary>
/// Immutable snapshot of a background job's current state.
/// Returned by GET /api/v1/jobs/{jobId} endpoints.
///
/// The record is designed to be stored in any key-value store
/// (IMemoryCache, Redis, PostgreSQL JSONB, etc.) serialized as JSON.
/// </summary>
public sealed record JobRecord
{
    /// <summary>Stable identifier assigned at enqueue time.</summary>
    public Guid JobId { get; init; }

    /// <summary>Current lifecycle state.</summary>
    public JobStatus Status { get; init; }

    /// <summary>
    /// JSON-serialized result payload.
    /// Populated only when <see cref="Status"/> is <see cref="JobStatus.Completed"/>.
    /// </summary>
    public string? Result { get; init; }

    /// <summary>
    /// Human-readable error description.
    /// Populated only when <see cref="Status"/> is <see cref="JobStatus.Failed"/>.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>When the job was enqueued (UTC).</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>When the job reached a terminal state (UTC). Null while still running.</summary>
    public DateTimeOffset? CompletedAt { get; init; }

    // ── Factory / transition helpers ─────────────────────────────────────────

    /// <summary>Creates a new job record in <see cref="JobStatus.Queued"/> state.</summary>
    public static JobRecord Create(Guid jobId) => new()
    {
        JobId     = jobId,
        Status    = JobStatus.Queued,
        CreatedAt = DateTimeOffset.UtcNow
    };

    /// <summary>Returns a copy of this record transitioned to <see cref="JobStatus.Running"/>.</summary>
    public JobRecord AsRunning() =>
        this with { Status = JobStatus.Running };

    /// <summary>
    /// Returns a copy of this record transitioned to <see cref="JobStatus.Completed"/>.
    /// </summary>
    /// <param name="result">Optional JSON-serialized result payload.</param>
    public JobRecord AsCompleted(string? result = null) =>
        this with { Status = JobStatus.Completed, Result = result, CompletedAt = DateTimeOffset.UtcNow };

    /// <summary>Returns a copy of this record transitioned to <see cref="JobStatus.Failed"/>.</summary>
    /// <param name="error">Human-readable error description.</param>
    public JobRecord AsFailed(string error) =>
        this with { Status = JobStatus.Failed, Error = error, CompletedAt = DateTimeOffset.UtcNow };
}
