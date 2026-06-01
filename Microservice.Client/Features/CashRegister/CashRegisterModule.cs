using FluentValidation;
using Microservice.Client.Features.CashRegister.Models;
using Microservice.Client.Features.CashRegister.Services;
using Microservice.Client.Features.CashRegister.State;
using Microservice.Client.Features.CashRegister.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Microservice.Client.Features.CashRegister;

/// <summary>DI for the CashRegister feature: gateway, active-session state, and form validators.</summary>
public static class CashRegisterModule
{
    public static IServiceCollection AddCashRegisterFeature(this IServiceCollection services)
    {
        services.AddScoped<ICashGateway, CashGateway>();
        services.AddScoped<CashSessionState>();

        services.AddTransient<IValidator<OpenSessionFormModel>, OpenSessionFormValidator>();
        services.AddTransient<IValidator<MovementFormModel>, MovementFormValidator>();
        services.AddTransient<IValidator<CloseSessionFormModel>, CloseSessionFormValidator>();
        return services;
    }
}
