using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.Dapper
{
    public interface IProductReadRepository : IReadRepository<Product>
    {
        Task<IReadOnlyList<Product>> SearchByNameAsync(string name, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    }
}