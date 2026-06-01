using Microservice.Client.Infrastructure.Http;
using Microservice.Client.Infrastructure.Resilience;
using Microservice.Client.Shared.Results;
using Microsoft.Extensions.Logging;

namespace Microservice.Client.Infrastructure.Offline.Sync;

/// <summary>
/// Drains the <see cref="ISyncQueue"/> when connectivity is available. Replays each operation
/// FIFO with its stable idempotency key; classifies the outcome and either removes the op
/// (delivered), keeps it for backoff (retryable), or flags it as a conflict (rejected).
///
/// Single-threaded by design (a POS terminal has one user); a semaphore guards against
/// overlapping drains triggered by both the timer and the online event.
/// </summary>
public sealed class SyncProcessor(
    ISyncQueue queue,
    ApiClient api,
    IConnectivity connectivity,
    ILogger<SyncProcessor> logger) : IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _started;

    /// <summary>Raised when an operation is permanently rejected — diagnostics surfaces it.</summary>
    public event Action<SyncOperation, UiError>? ConflictDetected;

    /// <summary>Begin reacting to connectivity changes. Idempotent — safe to call on every layout init.</summary>
    public void Start()
    {
        if (_started)
            return;
        _started = true;
        connectivity.StatusChanged += OnConnectivityChanged;
    }

    /// <summary>
    /// Stop reacting to connectivity changes WITHOUT disposing the processor. Components call this
    /// instead of DisposeAsync — the DI container owns this service's lifetime (it is effectively a
    /// singleton in WASM), so a component must never dispose its SemaphoreSlim.
    /// </summary>
    public void Stop()
    {
        if (!_started)
            return;
        _started = false;
        connectivity.StatusChanged -= OnConnectivityChanged;
    }

    private void OnConnectivityChanged(bool isOnline)
    {
        if (isOnline)
            _ = DrainAsync();
    }

    /// <summary>Attempt to deliver all pending operations. Safe to call repeatedly.</summary>
    public async Task DrainAsync(CancellationToken ct = default)
    {
        if (!connectivity.IsOnline || !await _gate.WaitAsync(0, ct))
            return;

        try
        {
            foreach (var op in await queue.GetPendingAsync())
            {
                if (ct.IsCancellationRequested || !connectivity.IsOnline)
                    break;

                var outcome = await DeliverAsync(op, ct);
                if (outcome is SyncOutcome.Retryable)
                    break; // preserve FIFO; try again on next drain after backoff
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<SyncOutcome> DeliverAsync(SyncOperation op, CancellationToken ct)
    {
        var result = await api.ReplayAsync(op.Method, op.Url, op.JsonBody, op.IdempotencyKey, ct);

        if (result.IsSuccess)
        {
            await queue.RemoveAsync(op.Id);
            return SyncOutcome.Delivered;
        }

        var error = result.Error!;
        var outcome = Classify(error.Kind);

        if (outcome is SyncOutcome.Rejected)
        {
            logger.LogWarning("Sync op {Id} ({Entity}) rejected: {Kind} {Message}",
                op.Id, op.EntityType, error.Kind, error.Message);
            await queue.RemoveAsync(op.Id);
            ConflictDetected?.Invoke(op, error);
            return outcome;
        }

        // Retryable: persist the failed attempt count + backoff before the next drain.
        var next = op with { Attempts = op.Attempts + 1, LastError = error.Message };
        await queue.UpdateAsync(next);
        await Task.Delay(RetryPolicy.Backoff(next.Attempts), ct);
        return outcome;
    }

    // 400/404/409/401 are deterministic — replaying won't help. Network/429/5xx are transient.
    private static SyncOutcome Classify(ErrorKind kind) => kind switch
    {
        ErrorKind.Validation or ErrorKind.Conflict or ErrorKind.NotFound or ErrorKind.Unauthorized
            => SyncOutcome.Rejected,
        _ => SyncOutcome.Retryable
    };

    /// <summary>Invoked by the DI container at app shutdown — the only place the gate is disposed.</summary>
    public ValueTask DisposeAsync()
    {
        Stop();
        _gate.Dispose();
        return ValueTask.CompletedTask;
    }
}
