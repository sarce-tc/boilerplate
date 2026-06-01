using Microservice.Client.Infrastructure.Http;
using Microservice.Client.Infrastructure.Offline;
using Microservice.Client.Infrastructure.Offline.IndexedDb;
using Microservice.Client.Infrastructure.Offline.Sync;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microservice.Client.Bootstrap;

/// <summary>
/// Infrastructure services: the typed HTTP pipeline (correlation, auth, idempotency),
/// the <see cref="ApiClient"/>, and the offline stack (IndexedDB, connectivity, sync).
/// </summary>
public static class ClientInfrastructureServiceCollectionExtensions
{
    private const string ApiClientName = "Api";

    public static IServiceCollection AddClientInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ── HTTP pipeline ──────────────────────────────────────────────────────
        services.AddScoped<CorrelationIdHandler>();
        services.AddScoped<AuthTokenHandler>();
        services.AddScoped<IdempotencyKeyHandler>();

        services.AddHttpClient(ApiClientName, (sp, client) =>
            {
                var options = sp.GetRequiredService<ApiOptions>();
                client.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddHttpMessageHandler<CorrelationIdHandler>()   // outermost: stamp X-Correlation-Id
            .AddHttpMessageHandler<AuthTokenHandler>()        // then attach bearer
            .AddHttpMessageHandler<IdempotencyKeyHandler>();  // then promote idempotency key

        services.AddScoped(sp =>
            new ApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(ApiClientName)));

        // ── Offline stack ──────────────────────────────────────────────────────
        services.AddScoped<IIndexedDb, IndexedDb>();
        services.AddScoped<IConnectivity, Connectivity>();
        services.AddScoped<ISyncQueue, SyncQueue>();
        services.AddScoped<SyncProcessor>();

        return services;
    }
}
