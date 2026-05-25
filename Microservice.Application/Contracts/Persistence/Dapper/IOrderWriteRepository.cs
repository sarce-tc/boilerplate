using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.Dapper;

public interface IOrderWriteRepository : IWriteRepository<Order>
{
    /// <summary>
    /// Inserts a single <see cref="OrderItem"/> row.
    /// Must be called inside an active UoW transaction so Order + Items
    /// are committed atomically.
    /// </summary>
    Task<OrderItem> AddItemAsync(OrderItem item, CancellationToken ct = default);
}
