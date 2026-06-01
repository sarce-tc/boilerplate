using FluentValidation;
using Microservice.Client.Features.Products.Models;
using Microservice.Client.Features.Products.Services;
using Microservice.Client.Features.Products.State;
using Microservice.Client.Features.Products.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Microservice.Client.Features.Products;

/// <summary>
/// Self-contained DI registration for the Products feature. Every feature exposes one of these
/// so the composition root just lists modules (open/closed: add a feature = add a module).
/// </summary>
public static class ProductsModule
{
    public static IServiceCollection AddProductsFeature(this IServiceCollection services)
    {
        services.AddScoped<IProductsGateway, ProductsGateway>();
        services.AddScoped<ProductsState>();
        services.AddTransient<IValidator<ProductFormModel>, ProductFormValidator>();
        return services;
    }
}
