namespace Microservice.Client.Infrastructure.Offline.Sync;

/// <summary>
/// Durable outbound queue for mutations. Gateways enqueue here when offline (or always, for
/// append-only flows); the <see cref="SyncProcessor"/> drains it. Observable so the
/// diagnostics panel can show pending count / last sync.
/// </summary>
public interface ISyncQueue
{
    /// <summary>Pending operation count changed (enqueue/drain). UI subscribes for the badge.</summary>
    event Action? Changed;

    ValueTask EnqueueAsync(SyncOperation operation);
    ValueTask<IReadOnlyList<SyncOperation>> GetPendingAsync();
    ValueTask RemoveAsync(string id);
    ValueTask UpdateAsync(SyncOperation operation);
    ValueTask<int> CountAsync();
}
