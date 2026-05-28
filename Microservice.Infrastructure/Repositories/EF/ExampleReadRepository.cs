using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;
using Microservice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Microservice.Infrastructure.Repositories.EF;

// ═══════════════════════════════════════════════════════════════════════
// AGENT — Repositorio de lectura EF Core del aggregate Example.
// Extiende LINQRepository<Example> con operaciones específicas del dominio.
// ExistsByNameAsync: verifica existencia con comparación ILike (case-insensitive)
//   sobre Name usando EF.Functions.ILike — fully-qualified para evitar colisión
//   con el namespace EF del proyecto.
// ═══════════════════════════════════════════════════════════════════════
public sealed class ExampleReadRepository(ExampleDbContext context)
    : LINQRepository<Example>(context),
      IExampleReadRepository
{
    /// <inheritdoc/>
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
        await _dbSet.AnyAsync(
            e => Microsoft.EntityFrameworkCore.EF.Functions.ILike(e.Name, name),
            ct);
}
