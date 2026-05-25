using Microservice.Application.Contracts.Jobs;
using Microservice.Application.Models.Jobs;
using Microsoft.Extensions.Caching.Memory;

namespace Microservice.Infrastructure.Jobs;

/// <summary>
/// In-memory implementation of <see cref="IJobStatusStore"/> backed by
/// <see cref="IMemoryCache"/>.
///
/// Jobs expire after 24 hours. For distributed / durable storage, replace
/// this with a Redis or PostgreSQL implementation.
/// </summary>
internal sealed class InMemoryJobStatusStore(IMemoryCache cache) : IJobStatusStore
{
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

    private static string CacheKey(Guid jobId) => $"job-status:{jobId:N}";

    public Task<JobRecord?> GetAsync(Guid jobId, CancellationToken ct = default)
    {
        cache.TryGetValue<JobRecord>(CacheKey(jobId), out var record);
        return Task.FromResult(record);
    }

    public Task SetAsync(Guid jobId, JobRecord record, CancellationToken ct = default)
    {
        cache.Set(CacheKey(jobId), record, Ttl);
        return Task.CompletedTask;
    }
}
