using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;
using Microservice.Infrastructure.Persistence;

namespace Microservice.Infrastructure.Repositories.EF;

// PATRÓN — Implementación del UoW EF Core que el agente debe replicar para nuevos aggregates.
// ═══════════════════════════════════════════════════════════════════════════
// AGENT — EF Core Unit of Work.
//
// · Scoped: una instancia por request; comparte ExampleDbContext con todos los repos.
// · Lazy init: el repositorio concreto se crea solo cuando se accede por primera vez.
//   Si el handler no escribe, el repo nunca se instancia.
// · Una sola instancia de repositorio concreto aunque se acceda por dos interfaces
//   distintas (IExampleWriteRepository y IWriteRepository<Example>) — ambas propiedades
//   apuntan al mismo objeto → sin doble tracking.
//
// Para agregar un nuevo aggregate (generic-first):
//   1. Si la superficie genérica de LINQRepository alcanza:
//      → No crear IMyEntityWriteRepository. Exponer solo:
//        public IWriteRepository<MyEntity> WriteRepository => _myEntity ??= new MyEntityWriteRepository(_context);
//   2. Si necesitas métodos de escritura específicos del aggregate:
//      → Crear IMyEntityWriteRepository : IWriteRepository<MyEntity>.
//      → Crear MyEntityWriteRepository : LINQRepository<MyEntity>, IMyEntityWriteRepository.
//      → Añadir ambas propiedades apuntando a la misma instancia lazy.
//      → Añadir la propiedad específica a IUnitOfWork.
// ═══════════════════════════════════════════════════════════════════════════
public sealed class UnitOfWork(ExampleDbContext context) : IUnitOfWork
{
    private ExampleWriteRepository? _examplesWrite;

    // Una sola instancia compartida expuesta por ambas interfaces sin doble tracking.
    private ExampleWriteRepository LazyExamplesWrite =>
        _examplesWrite ??= new ExampleWriteRepository(context);

    public IExampleWriteRepository   ExamplesWrite   => LazyExamplesWrite;
    public IWriteRepository<Example> WriteRepository => LazyExamplesWrite;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
