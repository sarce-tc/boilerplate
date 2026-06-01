using FluentValidation;
using Microservice.Client.Features.Inventory.Models;
using Microservice.Client.Features.Inventory.Services;
using Microservice.Client.Features.Inventory.State;
using Microservice.Client.Features.Inventory.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Microservice.Client.Features.Inventory;

/// <summary>DI for the Inventory feature (mirrors the archetype modules).</summary>
public static class InventoryModule
{
    public static IServiceCollection AddInventoryFeature(this IServiceCollection services)
    {
        services.AddScoped<IInventoryGateway, InventoryGateway>();
        services.AddScoped<InventoryState>();
        services.AddTransient<IValidator<MovementFormModel>, MovementFormValidator>();
        return services;
    }
}
