using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;
using Microservice.Infrastructure.Persistence;

namespace Microservice.Infrastructure.Repositories.EF;

// ═══════════════════════════════════════════════════════════════════════
// AGENT — Repositorio de escritura EF Core del aggregate Example.
// Extiende LINQRepository<Example>; todas las operaciones de escritura
// (AddAsync, Update, UpdateFields, DeleteManyAsync, etc.) las provee la clase base.
// La instancia es administrada por UnitOfWork como lazy property compartida.
// ═══════════════════════════════════════════════════════════════════════
public sealed class ExampleWriteRepository(ExampleDbContext context)
    : LINQRepository<Example>(context),
      IExampleWriteRepository
{
}
