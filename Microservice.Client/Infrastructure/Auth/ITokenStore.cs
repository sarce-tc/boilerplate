namespace Microservice.Client.Infrastructure.Auth;

/// <summary>
/// Persists the JWT across reloads. Single source of truth for the raw token — handlers,
/// the auth state provider, and the login service all go through here. Backed by
/// localStorage (survives PWA restarts; acceptable for a POS terminal — see security note
/// in AuthService). Swap the implementation to switch storage without touching callers.
/// </summary>
public interface ITokenStore
{
    ValueTask<string?> GetTokenAsync();
    ValueTask SetTokenAsync(string token, DateTimeOffset expiresAt);
    ValueTask ClearAsync();
    ValueTask<bool> HasValidTokenAsync();
}
