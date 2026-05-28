using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.Dapper;
// Contrato de lectura Dapper del aggregate Example.
// Extiende IReadRepository<Example> y agrega operaciones de búsqueda case-insensitive y verificación de unicidad.
public interface IExampleReadRepository : IReadRepository<Example>
{
    Task<IReadOnlyList<Example>> SearchByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
