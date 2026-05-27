using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.EF;

/// <summary>
/// PATRÓN — Unit of Work EF Core por bounded context.
/// </summary>
/// <remarks>
/// <para>
/// <b>Generic-first:</b> el punto de entrada por defecto para writes en un command handler
/// es <see cref="WriteRepository"/> (<c>IWriteRepository&lt;T&gt;</c> genérico).
/// Solo exponer una propiedad de tipo <c>IMyEntityWriteRepository</c> cuando el aggregate
/// necesita métodos de escritura que no existen en la superficie genérica.
/// </para>
/// <para>
/// <b>Cómo extender para una nueva entidad:</b>
/// <list type="number">
///   <item>Crear <c>IMyEntityWriteRepository : IWriteRepository&lt;MyEntity&gt;</c>.</item>
///   <item>Crear <c>MyEntityWriteRepository : LINQRepository&lt;MyEntity&gt;, IMyEntityWriteRepository</c>.</item>
///   <item>Añadir lazy property aquí: <c>IMyEntityWriteRepository MyEntityWrite {{ get; }}</c>.</item>
///   <item>Implementar en <c>Infrastructure/Repositories/EF/UnitOfWork.cs</c>.</item>
/// </list>
/// Si la superficie genérica alcanza, omitir el paso 1 y usar <see cref="WriteRepository"/> directamente.
/// </para>
/// <para>
/// <b>Sin TX explícita:</b> <see cref="SaveChangesAsync"/> envuelve todos los cambios
/// pendientes del DbContext en una transacción implícita de PostgreSQL.
/// </para>
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Repositorio de escritura específico del aggregate <see cref="Example"/>.
    /// <br/>
    /// PATRÓN: exponer una propiedad de este tipo solo cuando el aggregate necesita
    /// métodos de escritura que la superficie genérica <see cref="WriteRepository"/> no cubre.
    /// </summary>
    IExampleWriteRepository ExamplesWrite { get; }

    /// <summary>
    /// Superficie genérica de escritura. Punto de entrada por defecto para writes.
    /// Usar directamente para <c>DeleteManyAsync</c>, <c>UpdateManyAsync</c> y cualquier
    /// operación que no requiera métodos específicos del aggregate.
    /// </summary>
    IWriteRepository<Example> WriteRepository { get; }

    /// <summary>
    /// Confirma todos los cambios pendientes en una única transacción implícita.
    /// Llamar una sola vez al final del handler.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
