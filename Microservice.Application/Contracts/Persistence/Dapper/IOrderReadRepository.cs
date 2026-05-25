using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.Dapper;

public interface IOrderReadRepository : IReadRepository<Order>
{
    /// <summary>
    /// Loads an Order together with all its OrderItems via a single JOIN query.
    /// Returns a tuple so the caller can build the DTO without needing access
    /// to the private <c>Order._items</c> collection.
    /// </summary>
    Task<(Order? Order, IReadOnlyList<OrderItem> Items)> GetWithItemsAsync(
        Guid publicId,
        CancellationToken ct = default);
}
