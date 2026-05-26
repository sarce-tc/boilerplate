using Microservice.Application.DTOs.Orders;
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

    /// <summary>
    /// Returns a page of order summaries (with item count) and the total row count.
    /// Direct Dapper projection → returns <see cref="OrderSummaryDto"/> (not domain entities)
    /// because <c>ItemCount</c> is a SQL aggregate not present on <see cref="Order"/>.
    /// Uses <c>QueryMultipleAsync</c> for a single round-trip (data + count).
    /// </summary>
    Task<(IReadOnlyList<OrderSummaryDto> Orders, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct = default);
}
