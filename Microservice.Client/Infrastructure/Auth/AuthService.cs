using Microservice.Client.Infrastructure.Http;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Infrastructure.Auth;

/// <summary>
/// Authenticates against the backend token endpoint, persists the token, and notifies the
/// auth state provider so protected routes unlock immediately.
///
/// SECURITY NOTE: the JWT is stored in localStorage. That is the pragmatic choice for an
/// installed POS terminal (survives reloads, no httpOnly-cookie server needed against this
/// API). It is XSS-exposed by design; the mitigations are: strict CSP, no third-party
/// scripts, short token lifetime, and server-side authorization on every endpoint.
/// </summary>
public sealed class AuthService(
    ApiClient api,
    ITokenStore tokenStore,
    JwtAuthenticationStateProvider authStateProvider,
    ApiOptions options) : IAuthService
{
    public async Task<UiResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        // The token endpoint is [AllowAnonymous]; the store never yields an expired token,
        // so no stale bearer is attached. No need to flag the request anonymous here.
        var url = options.ResourcePath("auth/token");
        var result = await api.PostAsync<TokenResponse>(url, new TokenRequest(username, password), idempotencyKey: null, ct);

        if (result.IsFailure)
            return UiResult.Failure(result.Error!);

        var token = result.Value!;
        await tokenStore.SetTokenAsync(token.Token, token.ExpiresAt);
        authStateProvider.NotifyAuthenticationChanged();
        return UiResult.Success();
    }

    public async Task LogoutAsync()
    {
        await tokenStore.ClearAsync();
        authStateProvider.NotifyAuthenticationChanged();
    }
}
