using Microservice.Application.Contracts.Infrastructure;
using Microservice.Application.Contracts.Jobs;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Infrastructure.Cache;
using Microservice.Infrastructure.Jobs;
using Microservice.Infrastructure.Persistence;
using Microservice.Infrastructure.Repositories.Dapper;
using Microservice.Infrastructure.Repositories.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;

namespace Microservice.Infrastructure
{
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

            // EF repositories (generic)
            services.AddScoped(typeof(Application.Contracts.Persistence.EF.IReadRepository<>), typeof(LINQRepository<>));
            services.AddScoped(typeof(Application.Contracts.Persistence.EF.IWriteRepository<>), typeof(LINQRepository<>));
            services.AddScoped(typeof(IQueryRepository<>), typeof(LINQRepository<>));

            // EF raw SQL repositories
            services.AddScoped(typeof(ISqlQueryRepository<>),   typeof(SqlRepository<>));
            services.AddScoped(typeof(ISqlCommandRepository<>),  typeof(SqlRepository<>));
            services.AddScoped(typeof(ISqlRepository<>),         typeof(SqlRepository<>));

            // EF UoW (delegates to DbContext)
            services.AddScoped<Application.Contracts.Persistence.IUnitOfWork>(sp =>
                sp.GetRequiredService<ExampleDbContext>());

            // ── Dapper ────────────────────────────────────────────────────────
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true; // snake_case → PascalCase

            services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

            // Example aggregate
            services.AddScoped<IExampleReadRepository,  ExampleReadRepository>();
            services.AddScoped<IExampleWriteRepository, ExampleWriteRepository>();

            // Dapper UoW
            services.AddScoped<Application.Contracts.Persistence.Dapper.IUnitOfWork, UnitOfWork>();

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
}