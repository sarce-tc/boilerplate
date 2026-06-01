using Microservice.Client.Infrastructure.Offline.IndexedDb;

namespace Microservice.Client.Infrastructure.Offline.Sync;

/// <summary>
/// IndexedDB-backed implementation of <see cref="ISyncQueue"/>. Keyed by operation id;
/// ordering on drain is by <see cref="SyncOperation.CreatedAt"/> (FIFO) so dependent
/// mutations replay in the order they were made.
/// </summary>
public sealed class SyncQueue(IIndexedDb db) : ISyncQueue
{
    public event Action? Changed;

    public async ValueTask EnqueueAsync(SyncOperation operation)
    {
        await db.PutAsync(Stores.SyncQueue, operation.Id, operation);
        Changed?.Invoke();
    }

    public async ValueTask<IReadOnlyList<SyncOperation>> GetPendingAsync()
    {
        var all = await db.GetAllAsync<SyncOperation>(Stores.SyncQueue);
        return all.OrderBy(o => o.CreatedAt).ToList();
    }

    public async ValueTask RemoveAsync(string id)
    {
        await db.DeleteAsync(Stores.SyncQueue, id);
        Changed?.Invoke();
    }

    public async ValueTask UpdateAsync(SyncOperation operation) =>
        await db.PutAsync(Stores.SyncQueue, operation.Id, operation);

    public async ValueTask<int> CountAsync() =>
        (await db.GetAllAsync<SyncOperation>(Stores.SyncQueue)).Count;
}
