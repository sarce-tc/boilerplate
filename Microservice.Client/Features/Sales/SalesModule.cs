using Microservice.Client.Features.Sales.Services;
using Microservice.Client.Features.Sales.State;
using Microsoft.Extensions.DependencyInjection;

namespace Microservice.Client.Features.Sales;

/// <summary>DI for the Sales feature. (In WASM, Scoped == one instance per app, so the cart persists across navigations.)</summary>
public static class SalesModule
{
    public static IServiceCollection AddSalesFeature(this IServiceCollection services)
    {
        services.AddScoped<ISalesGateway, SalesGateway>();
        services.AddScoped<SaleCartState>();
        services.AddScoped<IBarcodeScanner, BarcodeScanner>();
        return services;
    }
}
