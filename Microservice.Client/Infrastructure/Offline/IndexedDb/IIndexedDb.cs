namespace Microservice.Client.Infrastructure.Offline.IndexedDb;

/// <summary>
/// Minimal typed facade over IndexedDB (the only durable, sizable store available in WASM —
/// localStorage is too small and synchronous). Object stores are keyed by string; values are
/// serialized as JSON by the JS layer. Read caches (catalog, customers) and the sync queue
/// both live here, in separate stores.
///
/// We hand-rolled a thin interop module instead of taking a NuGet wrapper so the schema,
/// store set, and versioning stay under our control (architecture decision).
/// </summary>
public interface IIndexedDb
{
    ValueTask PutAsync<T>(string store, string key, T value);
    ValueTask<T?> GetAsync<T>(string store, string key);
    ValueTask<IReadOnlyList<T>> GetAllAsync<T>(string store);
    ValueTask DeleteAsync(string store, string key);
    ValueTask ClearAsync(string store);
}

/// <summary>Canonical store names. One store per concern keeps eviction policies independent.</summary>
public static class Stores
{
    /// <summary>Read-only cached catalog/reference data (server-authoritative, revalidated).</summary>
    public const string ReadCache = "read-cache";

    /// <summary>Pending outbound mutations awaiting sync (append-only until drained).</summary>
    public const string SyncQueue = "sync-queue";
}
