using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Microservice.API.Controllers;

// ── DTOs (file-scoped, no need for separate files) ───────────────────────────

/// <summary>Credentials for token generation.</summary>
public sealed record TokenRequest(string Username, string Password);

/// <summary>Issued JWT token with its lifetime.</summary>
public sealed record TokenResponse(string Token, int ExpiresInSeconds, DateTimeOffset ExpiresAt);

/// <summary>
/// A configured test user (only read from appsettings.Development.json).
/// </summary>
internal sealed record TestUser(string Username, string Password, string Role = "User");

// ── Controller ────────────────────────────────────────────────────────────────

/// <summary>
/// Token endpoint for development and testing.
///
/// ⚠️ This controller generates JWT tokens using test users configured in
/// appsettings.Development.json → Jwt:TestUsers.
/// In production, Jwt:TestUsers should be empty / absent so no token
/// can be issued through this endpoint.
///
/// For production, replace this with your identity provider (Keycloak, Auth0,
/// Azure AD B2C, etc.) and remove or gate this controller.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[AllowAnonymous]
[Tags("Auth")]
public class AuthController(IConfiguration configuration, IWebHostEnvironment env) : ControllerBase
{
    private readonly IConfiguration     _configuration = configuration;
    private readonly IWebHostEnvironment _env          = env;

    /// <summary>
    /// Generate a JWT Bearer token (development / testing only).
    ///
    /// Credentials are validated against <c>Jwt:TestUsers</c> in appsettings.
    /// Returns a signed HS256 token valid for <c>Jwt:ExpirationMinutes</c>.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/auth/token
    ///     {
    ///         "username": "admin",
    ///         "password": "admin123"
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Token issued successfully.</response>
    /// <response code="400">Invalid credentials or no TestUsers configured.</response>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult GenerateToken([FromBody] TokenRequest request)
    {
        var users = _configuration
            .GetSection("Jwt:TestUsers")
            .Get<List<TestUser>>();

        var user = users?.FirstOrDefault(u =>
            string.Equals(u.Username, request.Username, StringComparison.OrdinalIgnoreCase) &&
            u.Password == request.Password);

        if (user is null)
            return BadRequest(new { error = "Invalid credentials or TestUsers not configured." });

        var (token, expiresAt) = IssueToken(user);

        return Ok(new TokenResponse(
            Token:          token,
            ExpiresInSeconds: (int)(expiresAt - DateTimeOffset.UtcNow).TotalSeconds,
            ExpiresAt:      expiresAt));
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private (string token, DateTimeOffset expiresAt) IssueToken(TestUser user)
    {
        var jwtSection = _configuration.GetSection("Jwt");

        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expMinutes  = int.Parse(jwtSection["ExpirationMinutes"] ?? "60");
        var expiresAt   = DateTimeOffset.UtcNow.AddMinutes(expMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name,              user.Username),
            new Claim(ClaimTypes.Role,              user.Role)
        };

        var token = new JwtSecurityToken(
            issuer:             jwtSection["Issuer"],
            audience:           jwtSection["Audience"],
            claims:             claims,
            expires:            expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
