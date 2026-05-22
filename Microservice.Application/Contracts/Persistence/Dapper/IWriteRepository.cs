using Microservice.Domain.Common;

namespace Microservice.Application.Contracts.Persistence.Dapper
{
    public interface IWriteRepository<T> where T : BaseDomainModel
    {
        Task<T> AddAsync(T entity, CancellationToken ct = default);
        Task<T> UpdateAsync(T entity, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
