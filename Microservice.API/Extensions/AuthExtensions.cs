using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Microservice.API.Extensions;

/// <summary>
/// JWT Bearer authentication configuration.
///
/// Reads from config section "Jwt":
///   Key               — HS256 symmetric key, min 32 chars
///   Issuer            — token issuer (validated on every request)
///   Audience          — token audience (validated on every request)
///   ExpirationMinutes — default token lifetime (used by AuthController)
///
/// Usage:
///   services.AddJwtAuthentication(configuration);
///   // then in middleware pipeline:
///   app.UseAuthentication();
///   app.UseAuthorization();
/// </summary>
public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");

        var rawKey = jwtSection["Key"]
            ?? throw new InvalidOperationException(
                "Jwt:Key is not configured. " +
                "Set it in appsettings or via the JWT_KEY environment variable.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Key
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = key,

                    // Issuer / Audience
                    ValidateIssuer   = true,
                    ValidIssuer      = jwtSection["Issuer"],
                    ValidateAudience = true,
                    ValidAudience    = jwtSection["Audience"],

                    // Lifetime — allow 30 s clock drift between nodes
                    ValidateLifetime = true,
                    ClockSkew        = TimeSpan.FromSeconds(30)
                };

                // Return a proper ProblemDetails on 401 instead of an empty response
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/problem+json";

                        var problem = new
                        {
                            type   = "https://httpstatuses.com/401",
                            title  = "Unauthorized",
                            detail = "A valid Bearer token is required.",
                            status = 401
                        };
                        await context.Response.WriteAsJsonAsync(problem,
                            options: null,
                            contentType: "application/problem+json",
                            cancellationToken: context.HttpContext.RequestAborted);
                    }
                };
            });

        // Secure-by-default: every endpoint requires an authenticated user unless
        // it is explicitly marked [AllowAnonymous] (e.g. AuthController, health checks).
        // Without a FallbackPolicy the JWT is configured but never enforced.
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
