using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.Dapper
{
    public interface IExampleReadRepository : IReadRepository<Example>
    {
        Task<IReadOnlyList<Example>> SearchByNameAsync(string name, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    }
}
