using Microservice.Client.Features.CashRegister;
using Microservice.Client.Features.Customers;
using Microservice.Client.Features.Inventory;
using Microservice.Client.Features.Products;
using Microservice.Client.Features.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace Microservice.Client.Bootstrap;

/// <summary>
/// Feature composition root. Each vertical registers itself through its own module; this is
/// the single list that grows as the POS gains modules (Sales, Inventory, Customers, …).
/// </summary>
public static class ClientFeaturesServiceCollectionExtensions
{
    public static IServiceCollection AddClientFeatures(this IServiceCollection services)
    {
        services.AddProductsFeature();
        services.AddCashRegisterFeature();
        services.AddSalesFeature();
        services.AddCustomersFeature();
        services.AddInventoryFeature();
        // services.AddReportsFeature();
        return services;
    }
}
