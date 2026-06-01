namespace Microservice.Client.Infrastructure.Auth;

/// <summary>Request body for POST /api/v1/auth/token. Mirrors backend TokenRequest.</summary>
public sealed record TokenRequest(string Username, string Password);

/// <summary>Response from POST /api/v1/auth/token. Mirrors backend TokenResponse.</summary>
public sealed record TokenResponse(string Token, int ExpiresInSeconds, DateTimeOffset ExpiresAt);
