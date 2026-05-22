using Microservice.Domain.Common;

namespace Microservice.Application.Contracts.Persistence.Dapper
{
    public interface IReadRepository<T> where T : BaseDomainModel
    {
        Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<T?> GetByPublicIdAsync(Guid publicId, CancellationToken ct = default);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
        Task<int> CountAsync(CancellationToken ct = default);
    }

}
