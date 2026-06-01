using FluentValidation;
using Microservice.Client.Features.Customers.Models;
using Microservice.Client.Features.Customers.Services;
using Microservice.Client.Features.Customers.State;
using Microservice.Client.Features.Customers.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Microservice.Client.Features.Customers;

/// <summary>DI for the Customers feature (mirrors ProductsModule).</summary>
public static class CustomersModule
{
    public static IServiceCollection AddCustomersFeature(this IServiceCollection services)
    {
        services.AddScoped<ICustomersGateway, CustomersGateway>();
        services.AddScoped<CustomersState>();
        services.AddTransient<IValidator<CustomerFormModel>, CustomerFormValidator>();
        return services;
    }
}
