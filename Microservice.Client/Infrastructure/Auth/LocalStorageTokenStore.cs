using Microsoft.JSInterop;

namespace Microservice.Client.Infrastructure.Auth;

/// <summary>
/// localStorage-backed token store via JS interop. Keeps the token and its expiry so the
/// app can decide whether to resume a session or send the user to /login on startup.
/// </summary>
public sealed class LocalStorageTokenStore(IJSRuntime js) : ITokenStore
{
    private const string TokenKey = "pos.auth.token";
    private const string ExpiryKey = "pos.auth.expiresAt";

    public async ValueTask<string?> GetTokenAsync()
    {
        if (!await HasValidTokenAsync())
            return null;
        return await js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
    }

    public async ValueTask SetTokenAsync(string token, DateTimeOffset expiresAt)
    {
        await js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        await js.InvokeVoidAsync("localStorage.setItem", ExpiryKey, expiresAt.ToUnixTimeSeconds().ToString());
    }

    public async ValueTask ClearAsync()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await js.InvokeVoidAsync("localStorage.removeItem", ExpiryKey);
    }

    public async ValueTask<bool> HasValidTokenAsync()
    {
        var raw = await js.InvokeAsync<string?>("localStorage.getItem", ExpiryKey);
        if (!long.TryParse(raw, out var unix))
            return false;
        // 30s skew so we don't hand out a token that dies mid-request.
        return DateTimeOffset.FromUnixTimeSeconds(unix) > DateTimeOffset.UtcNow.AddSeconds(30);
    }
}
