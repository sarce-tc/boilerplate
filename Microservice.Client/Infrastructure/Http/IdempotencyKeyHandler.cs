namespace Microservice.Client.Infrastructure.Http;

/// <summary>
/// Promotes the per-request <see cref="RequestOptions.IdempotencyKey"/> (set by the gateway
/// or SyncQueue) onto the <c>Idempotency-Key</c> header the backend middleware understands.
/// The handler never invents a key: a key is meaningful only if it is stable across retries
/// of the same logical command, which is the caller's responsibility.
/// </summary>
public sealed class IdempotencyKeyHandler : DelegatingHandler
{
    private const string Header = "Idempotency-Key";

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Options.TryGetValue(RequestOptions.IdempotencyKey, out var key) &&
            !string.IsNullOrWhiteSpace(key))
        {
            request.Headers.Remove(Header);
            request.Headers.Add(Header, key);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
