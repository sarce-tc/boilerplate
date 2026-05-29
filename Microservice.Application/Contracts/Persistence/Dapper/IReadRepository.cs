using Microservice.Application.Models;
using Microservice.Domain.ValueObjects;

namespace Microservice.Application.Contracts.Persistence.Dapper;
// Contrato de lectura genérico Dapper para cualquier aggregate.
// Extiende BaseDomainModel; la implementación base vive en ReadRepository<T>.
public interface IReadRepository<T> where T : BaseDomainModel
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<T?> GetByPublicIdAsync(Guid publicId, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<T>> GetListPaginatedAsync(int currentPage, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}
