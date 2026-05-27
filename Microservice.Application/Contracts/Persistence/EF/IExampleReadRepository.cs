using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.EF;

/// <summary>
/// PATRÓN — Contrato de lectura específico del aggregate.
/// </summary>
/// <remarks>
/// <para>
/// <b>Generic-first — cuándo crear esta interfaz para una nueva entidad:</b>
/// solo cuando el handler o el validator necesita queries que no existen en
/// <see cref="IReadRepository{T}"/> — por ejemplo filtros con lógica de negocio específica
/// que no se pueden expresar con el predicado genérico (<see cref="ExistsByNameAsync"/>
/// usa ILike case-insensitive, que no está en la superficie de <see cref="IReadRepository{T}"/>).
/// Para eager-loading de hijos usar <c>GetEntityAsync</c> con <c>includeProperties</c>
/// — no crear un método específico.
/// </para>
/// <para>
/// <b>Cómo crear la equivalente para una nueva entidad:</b>
/// <code>
/// public interface IMyEntityReadRepository : IReadRepository&lt;MyEntity&gt;
/// {
///     Task&lt;bool&gt; ExistsByNameAsync(string name, CancellationToken ct = default);
/// }
/// </code>
/// Registrar en DI como Scoped, independiente del UoW.
/// </para>
/// </remarks>
public interface IExampleReadRepository : IReadRepository<Example>
{
    /// <summary>
    /// PATRÓN — Query con lógica de negocio específica del aggregate (unicidad case-insensitive).
    /// Usar en validators de Create/Update para garantizar reglas que no existen en la superficie genérica.
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
