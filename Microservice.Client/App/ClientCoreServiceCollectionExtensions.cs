using Microservice.Client.Infrastructure.Auth;
using Microservice.Client.Infrastructure.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace Microservice.Client.Bootstrap;

/// <summary>
/// Core services: configuration binding, design system, and authentication primitives.
/// Kept separate from infrastructure so the composition root reads as layers.
/// </summary>
public static class ClientCoreServiceCollectionExtensions
{
    public static IServiceCollection AddClientCore(
        this IServiceCollection services, IConfiguration configuration, string hostBaseAddress)
    {
        // ── Configuration ──────────────────────────────────────────────────────
        var apiOptions = new ApiOptions();
        configuration.GetSection(ApiOptions.SectionName).Bind(apiOptions);
        if (string.IsNullOrWhiteSpace(apiOptions.BaseUrl))
            apiOptions.BaseUrl = hostBaseAddress; // fallback: same origin as the app
        services.AddSingleton(apiOptions);

        // ── Design system ──────────────────────────────────────────────────────
        services.AddMudServices();

        // ── Diagnostics ────────────────────────────────────────────────────────
        services.AddScoped<CorrelationContext>();

        // ── Authentication ─────────────────────────────────────────────────────
        services.AddAuthorizationCore();
        services.AddScoped<ITokenStore, LocalStorageTokenStore>();
        services.AddScoped<JwtAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<JwtAuthenticationStateProvider>());
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
