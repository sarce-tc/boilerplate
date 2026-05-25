using Dapper;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.Entities;
using Npgsql;

namespace Microservice.Infrastructure.Repositories.Dapper;

/// <summary>
/// Dapper read repository for Orders.
///
/// Overrides <see cref="ReadRepository{T}.GetByPublicIdAsync"/> to use the
/// correct snake_case column name. Adds <see cref="GetWithItemsAsync"/> that
/// loads the order + all items in one JOIN query using Dapper multi-mapping.
/// </summary>
public sealed class OrderReadRepository : ReadRepository<Order>, IOrderReadRepository
{
    protected override string TableName => "orders";

    public OrderReadRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory) { }

    // ── IOrderReadRepository ──────────────────────────────────────────────────
    // Note: base class ReadRepository<Order> already provides GetByPublicIdAsync
    // using "WHERE public_id = @PublicId" which works correctly with snake_case
    // columns thanks to DefaultTypeMap.MatchNamesWithUnderscores = true.

    /// <summary>
    /// Single JOIN query that returns both the Order row and all OrderItem rows.
    /// Uses Dapper multi-mapping with <c>splitOn: "id"</c> — Dapper splits on
    /// the second occurrence of the "id" column (the first belongs to Order).
    ///
    /// Returns a tuple so the caller can build DTOs without needing access to
    /// the private <c>Order._items</c> list.
    /// </summary>
    public async Task<(Order? Order, IReadOnlyList<OrderItem> Items)> GetWithItemsAsync(
        Guid publicId,
        CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = """
            SELECT
                o.id, o.public_id, o.customer_name, o.status, o.total_amount,
                o.created_at, o.updated_at,
                i.id, i.public_id, i.order_id, i.product_name,
                i.quantity, i.unit_price, i.line_total, i.created_at, i.updated_at
            FROM orders o
            LEFT JOIN order_items i ON i.order_id = o.id
            WHERE o.public_id = @PublicId
            ORDER BY i.id
            """;

        Order? order = null;
        var items = new List<OrderItem>();

        // Dapper multi-map: splits on the second "id" column (= OrderItem.id)
        await conn.QueryAsync<Order, OrderItem, Order>(
            sql,
            (o, item) =>
            {
                order ??= o;
                if (item is not null)
                    items.Add(item);
                return o;
            },
            new { PublicId = publicId },
            splitOn: "id");

        if (order is not null)
            order.RecalculateTotal(items);

        return (order, items.AsReadOnly());
    }
}
