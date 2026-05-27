using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;
using Microservice.Infrastructure.Persistence;

namespace Microservice.Infrastructure.Repositories.EF;

/// <summary>
/// PATRÓN — Repositorio de escritura específico del aggregate, para replicar en nuevas entidades.
/// </summary>
/// <remarks>
/// <para>
/// <b>Generic-first — cuándo crear esta clase para una nueva entidad:</b>
/// solo cuando el aggregate necesita métodos de escritura que no existen en
/// <see cref="LINQRepository{T}"/> (<c>AddAsync</c>, <c>Update</c>, <c>UpdateFields</c>,
/// <c>UpdateManyAsync</c>, <c>Delete</c>, <c>DeleteManyAsync</c>).
/// Si ningún método adicional es necesario, el body queda vacío y la clase solo declara herencia.
/// </para>
/// <para>
/// <b>Cómo replicar para una nueva entidad:</b>
/// <code>
/// public sealed class MyEntityWriteRepository(ExampleDbContext context)
///     : LINQRepository&lt;MyEntity&gt;(context),
///       IMyEntityWriteRepository   // omitir si no hay métodos específicos
/// {
///     // Métodos adicionales solo si son necesarios.
/// }
/// </code>
/// Registrar la instancia en <c>UnitOfWork</c> como lazy property.
/// </para>
/// </remarks>
public sealed class ExampleWriteRepository(ExampleDbContext context)
    : LINQRepository<Example>(context),
      IExampleWriteRepository
{
}
