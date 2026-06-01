using Microservice.Client.Infrastructure.Auth;

namespace Microservice.Client.Infrastructure.Http;

/// <summary>
/// Attaches the JWT as <c>Authorization: Bearer</c> on every request, unless the request
/// is flagged anonymous (e.g. the token endpoint itself). Reads from the token store so
/// components and gateways never touch the token directly.
/// </summary>
public sealed class AuthTokenHandler(ITokenStore tokenStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var isAnonymous = request.Options.TryGetValue(RequestOptions.Anonymous, out var anon) && anon;
        if (!isAnonymous)
        {
            var token = await tokenStore.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
