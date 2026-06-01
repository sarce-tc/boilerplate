using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Microservice.Client.Infrastructure.Auth;

/// <summary>
/// Builds the <see cref="AuthenticationState"/> from the stored JWT by decoding its
/// claims locally (no extra round-trip). The token is HS256-signed by the backend; the
/// client does NOT verify the signature — it only reads claims for UI/role decisions.
/// Every API call is still authorized server-side, so a tampered local token gains nothing.
/// </summary>
public sealed class JwtAuthenticationStateProvider(ITokenStore tokenStore) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        var identity = new ClaimsIdentity(ParseClaims(token), authenticationType: "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    /// <summary>Call after a successful login so the router/UI re-evaluate immediately.</summary>
    public void NotifyAuthenticationChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3)
            return [];

        var json = Base64UrlDecode(parts[1]);
        var map = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        if (map is null)
            return [];

        return map.SelectMany(ToClaims);
    }

    private static IEnumerable<Claim> ToClaims(KeyValuePair<string, JsonElement> kvp) =>
        kvp.Value.ValueKind == JsonValueKind.Array
            ? kvp.Value.EnumerateArray().Select(e => new Claim(Normalize(kvp.Key), e.ToString()))
            : [new Claim(Normalize(kvp.Key), kvp.Value.ToString())];

    // Map common JWT short names to the ClaimTypes the AuthorizeView/[Authorize(Roles=...)] expect.
    private static string Normalize(string name) => name switch
    {
        "role" or "roles" => ClaimTypes.Role,
        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" => ClaimTypes.Role,
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => ClaimTypes.Name,
        "unique_name" or "sub" => ClaimTypes.Name,
        _ => name
    };

    private static string Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(s));
    }
}
