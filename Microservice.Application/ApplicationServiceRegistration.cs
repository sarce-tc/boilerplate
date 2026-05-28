using FluentValidation;
using MediatR;
using Microservice.Application.Behaviours;
using Microservice.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Microservice.Application;
public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        // ── Application services ──────────────────────────────────────────────
        services.AddScoped<IExampleService, ExampleService>();

        return services;
    }
}
