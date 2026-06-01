namespace Microservice.Client.Infrastructure.Http;

/// <summary>
/// Attaches an <c>X-Correlation-Id</c> to every outgoing request (originated client-side)
/// and records it for diagnostics. The backend's CorrelationIdMiddleware honours an
/// inbound id and echoes it back, so client and server logs share the same trace key.
/// </summary>
public sealed class CorrelationIdHandler(CorrelationContext context) : DelegatingHandler
{
    private const string Header = "X-Correlation-Id";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = CorrelationContext.NewId();
        request.Headers.Remove(Header);
        request.Headers.Add(Header, correlationId);
        context.LastCorrelationId = correlationId;

        var response = await base.SendAsync(request, cancellationToken);

        // Prefer the server echo (authoritative) when present.
        if (response.Headers.TryGetValues(Header, out var echoed))
            context.LastCorrelationId = echoed.FirstOrDefault() ?? correlationId;

        return response;
    }
}
