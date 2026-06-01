namespace Microservice.Client.Infrastructure.Offline;

/// <summary>
/// Online/offline signal sourced from the browser (navigator.onLine + online/offline events).
/// The SyncProcessor listens to <see cref="StatusChanged"/> to drain the queue when the
/// connection returns; the diagnostics panel shows the current state.
/// </summary>
public interface IConnectivity : IAsyncDisposable
{
    bool IsOnline { get; }
    event Action<bool>? StatusChanged;
    Task InitializeAsync();
}
