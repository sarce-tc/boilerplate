using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.Dapper;
// Contrato de escritura Dapper del aggregate Example.
// Extiende IWriteRepository<Example> sin agregar operaciones específicas adicionales por ahora.
public interface IExampleWriteRepository : IWriteRepository<Example>
{
    // Si en el futuro necesitás algo específico de escritura para Example
    // Task<int> DeactivateAllAsync(CancellationToken ct = default);
}
