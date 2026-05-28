using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.EF;
// Contrato de lectura EF Core del aggregate Example.
// Extiende IReadRepository<Example> con operaciones específicas del dominio.
// ExistsByNameAsync: verifica unicidad de nombre con comparación ILike (case-insensitive);
//   usar en validators de Create/Update antes de persistir.
public interface IExampleReadRepository : IReadRepository<Example>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
