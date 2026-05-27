using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;
using Microservice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Microservice.Infrastructure.Repositories.EF;

/// <summary>
/// PATRÓN — Repositorio de lectura específico del aggregate, para replicar en nuevas entidades.
/// </summary>
/// <remarks>
/// <para>
/// <b>Generic-first — cuándo crear esta clase para una nueva entidad:</b>
/// solo cuando el handler o el validator necesita queries que no existen en
/// <see cref="LINQRepository{T}"/> — por ejemplo filtros con lógica de negocio específica.
/// Para eager-loading de hijos usar <c>GetEntityAsync</c> con <c>includeProperties</c> —
/// no crear un método específico.
/// Para <c>FindAsync</c>, <c>GetEntityAsync</c>, <c>GetListPaginatedAsync</c>, etc.,
/// el handler inyecta <c>IReadRepository&lt;T&gt;</c> directamente sin necesitar esta clase.
/// </para>
/// <para>
/// <b>Cómo replicar para una nueva entidad:</b>
/// <code>
/// public sealed class MyEntityReadRepository(ExampleDbContext context)
///     : LINQRepository&lt;MyEntity&gt;(context),
///       IMyEntityReadRepository
/// {
///     public async Task&lt;bool&gt; ExistsByNameAsync(string name, CancellationToken ct = default) =>
///         await _dbSet.AnyAsync(
///             e => Microsoft.EntityFrameworkCore.EF.Functions.ILike(e.Name, name), ct);
/// }
/// </code>
/// Registrar en DI como Scoped, independiente del UoW.
/// </para>
/// <para>
/// <b>Nota sobre <c>ILike</c>:</b> usar <c>Microsoft.EntityFrameworkCore.EF.Functions.ILike</c>
/// (fully qualified) para evitar el conflicto de nombres con el namespace <c>EF</c> del proyecto.
/// </para>
/// </remarks>
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
