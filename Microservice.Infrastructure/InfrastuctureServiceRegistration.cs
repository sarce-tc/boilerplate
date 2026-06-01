using Microservice.Application.Contracts.Infrastructure;
using Microservice.Application.Contracts.Interfaces;
using Microservice.Application.Contracts.Jobs;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Infrastructure.Cache;
using Microservice.Infrastructure.Jobs;
using Microservice.Infrastructure.Persistence;
using Microservice.Infrastructure.Repositories.Dapper;
using Microservice.Infrastructure.Repositories.EF;
using Microservice.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;

namespace Microservice.Infrastructure;
// ═══════════════════════════════════════════════════════════════════════
// AGENT — DI registration for all infrastructure services.
//
// To register a NEW aggregate's Dapper repos, add two lines under "Order" block:
//   services.AddScoped<IMyEntityReadRepository,  MyEntityReadRepository>();
//   services.AddScoped<IMyEntityWriteRepository, MyEntityWriteRepository>();
// Write repos are also instantiated inside UnitOfWork (lazy, shared connection).
//
// Key registrations:
//   IDbConnectionFactory (Singleton) → creates NpgsqlConnection per request
//   IUnitOfWork (Scoped, Dapper)     → UnitOfWork; manages TX lifecycle
//   IJobQueue / IJobStatusStore      → Singleton in-memory job infrastructure
//   JobWorker                        → BackgroundService; drains the job channel
// ═══════════════════════════════════════════════════════════════════════
public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core — SQL logging controlled via Serilog appsettings
        // (Microsoft.EntityFrameworkCore.Database.Command → Information)
        services.AddDbContext<ExampleDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Caching
        services.AddMemoryCache();
        services.AddScoped<ICacheService, MemoryCacheService>();

        // Facturación electrónica AFIP/ARCA — STUB (reemplazar por WSAA/WSFEv1 real con certificados)
        services.AddScoped<IElectronicInvoicingService, StubElectronicInvoicingService>();

        // Impresión de tickets — render HTML (una impresora ESC/POS implementaría el mismo puerto)
        services.AddScoped<ITicketPrinter, HtmlTicketPrinter>();

        // EF repositories (generic)
        services.AddScoped(typeof(Application.Contracts.Persistence.EF.IReadRepository<>), typeof(LINQRepository<>));
        services.AddScoped(typeof(Application.Contracts.Persistence.EF.IWriteRepository<>), typeof(LINQRepository<>));
        services.AddScoped(typeof(IQueryRepository<>), typeof(LINQRepository<>));

        // EF raw SQL repositories
        services.AddScoped(typeof(ISqlQueryRepository<>),   typeof(SqlRepository<>));
        services.AddScoped(typeof(ISqlCommandRepository<>),  typeof(SqlRepository<>));
        services.AddScoped(typeof(ISqlRepository<>),         typeof(SqlRepository<>));

        // Example aggregate — EF repos
        services.AddScoped<Application.Contracts.Persistence.EF.IExampleReadRepository, Repositories.EF.ExampleReadRepository>();

        // Domain services (cross-aggregate)
        services.AddScoped<IExampleService, ExampleService>();
        services.AddScoped<IInventoryDomainService, InventoryDomainService>();
        services.AddScoped<ISaleDomainService, SaleDomainService>();

        // EF UoW
        services.AddScoped<Application.Contracts.Persistence.EF.IUnitOfWork, Repositories.EF.UnitOfWork>();

        // ── Dapper ────────────────────────────────────────────────────────
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true; // snake_case → PascalCase

        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

        // Example aggregate — Dapper repos
        services.AddScoped<Application.Contracts.Persistence.Dapper.IExampleReadRepository,  Repositories.Dapper.ExampleReadRepository>();
        services.AddScoped<Application.Contracts.Persistence.Dapper.IExampleWriteRepository, Repositories.Dapper.ExampleWriteRepository>();

        // Dapper UoW
        services.AddScoped<Application.Contracts.Persistence.Dapper.IUnitOfWork, Repositories.Dapper.UnitOfWork>();

        // ── Background Jobs ───────────────────────────────────────────────
        // Use Channel.CreateBounded<>(capacity) instead for backpressure.
        services.AddSingleton(Channel.CreateUnbounded<ChannelWorkItem>(
            new UnboundedChannelOptions { SingleReader = true }));

        services.AddSingleton<IJobStatusStore, InMemoryJobStatusStore>();
        services.AddSingleton<IJobQueue,       InMemoryJobQueue>();
        services.AddHostedService<JobWorker>();

        return services;
    }
}
