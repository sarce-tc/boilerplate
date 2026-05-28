using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.Dapper;
public interface IExampleWriteRepository : IWriteRepository<Example>
{
    // Si en el futuro necesitás algo específico de escritura para Example
    // Task<int> DeactivateAllAsync(CancellationToken ct = default);
}
