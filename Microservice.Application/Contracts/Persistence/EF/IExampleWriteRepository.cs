using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.EF;

/// <summary>
/// PATRÓN — Contrato de escritura específico del aggregate.
/// </summary>
/// <remarks>
/// <para>
/// <b>Generic-first — cuándo crear esta interfaz para una nueva entidad:</b>
/// solo cuando necesitas métodos de escritura que no existen en
/// <see cref="IWriteRepository{T}"/> (<c>AddAsync</c>, <c>Update</c>,
/// <c>UpdateFields</c>, <c>UpdateManyAsync</c>, <c>Delete</c>, <c>DeleteManyAsync</c>).
/// Si la superficie genérica alcanza, el handler usa <c>IUnitOfWork.WriteRepository</c>
/// directamente y esta interfaz no hace falta.
/// </para>
/// <para>
/// <b>Cómo crear la equivalente para una nueva entidad:</b>
/// <code>
/// public interface IMyEntityWriteRepository : IWriteRepository&lt;MyEntity&gt;
/// {
///     // Agregar solo métodos que no existen en la superficie genérica.
///     // Si no hay ninguno, dejar el body vacío — la implementación hereda todo de LINQRepository.
/// }
/// </code>
/// </para>
/// </remarks>
public interface IExampleWriteRepository : IWriteRepository<Example>
{
    // Agregar aquí métodos de escritura específicos del aggregate Example
    // que no existen en IWriteRepository<T>.
    // Si la superficie genérica es suficiente, dejar vacío.
}
