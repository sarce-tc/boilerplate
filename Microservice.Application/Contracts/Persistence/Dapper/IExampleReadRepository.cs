using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.Dapper;
// Contrato de lectura Dapper del aggregate Example.
// Extiende IReadRepository<Example> con operaciones específicas del dominio.
// SearchByNameAsync: búsqueda case-insensitive (ILike) por nombre; retorna colección de coincidencias.
// ExistsByNameAsync: verifica unicidad de nombre con comparación case-insensitive; usar en validators.
public interface IExampleReadRepository : IReadRepository<Example>
{
    Task<IReadOnlyList<Example>> SearchByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
