using Microservice.Application.Models.Jobs;

namespace Microservice.Application.Contracts.Jobs;

/// <summary>
/// Reads and writes job execution status snapshots.
///
/// Workers call <see cref="SetAsync"/> to transition a job through its lifecycle
/// (Queued → Running → Completed | Failed).
/// Controllers / clients call <see cref="GetAsync"/> to poll for the current state.
///
/// Concrete implementations:
/// - <b>In-process</b>  — <c>IMemoryCache</c> or <c>ConcurrentDictionary</c>
/// - <b>Distributed</b> — Redis (<c>IDistributedCache</c>), PostgreSQL, Cosmos DB
/// </summary>
public interface IJobStatusStore
{
    /// <summary>
    /// Returns the current <see cref="JobRecord"/> for <paramref name="jobId"/>,
    /// or <c>null</c> if no such job exists (e.g. expired or never enqueued).
    /// </summary>
    Task<JobRecord?> GetAsync(Guid jobId, CancellationToken ct = default);

    /// <summary>
    /// Persists (creates or overwrites) the <see cref="JobRecord"/> for
    /// <paramref name="jobId"/>.
    /// </summary>
    Task SetAsync(Guid jobId, JobRecord record, CancellationToken ct = default);
}
