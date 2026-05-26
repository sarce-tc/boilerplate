using Dapper;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Orders;
using Microservice.Domain.Entities;
using Npgsql;

namespace Microservice.Infrastructure.Repositories.Dapper;

// ═══════════════════════════════════════════════════════════════════════════
// AGENT ENTRY POINT — Dapper read path for Orders
//
// Inherited generic methods (ReadRepository<Order>):
//   GetByIdAsync(int id)               → SELECT * FROM orders WHERE id = @Id
//   GetByPublicIdAsync(Guid publicId)  → SELECT * FROM orders WHERE public_id = @PublicId
//   GetAllAsync()                      → SELECT * FROM orders  (no pagination)
//   ExistsAsync(int id)
//   CountAsync()
//
// Specific methods (override base table name = "orders"):
//   GetWithItemsAsync(Guid)   → single JOIN query, Dapper multi-map, splitOn: "id"
//                               returns (Order?, IReadOnlyList<OrderItem>)
//                               use this when you need items (detail view, RemoveOrderItem)
//   GetPagedAsync(page, size) → QueryMultipleAsync: page rows + total count in one round-trip
//                               returns (IReadOnlyList<OrderSummaryDto>, int totalCount)
//
// Snake_case mapping: DefaultTypeMap.MatchNamesWithUnderscores = true (set at startup)
// → public_id → PublicId, item_count → ItemCount, etc.
// ═══════════════════════════════════════════════════════════════════════════
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

    /// <summary>
    /// Single round-trip using <c>QueryMultipleAsync</c>:
    /// — first result set  → page of <see cref="OrderSummaryDto"/> rows (with item_count aggregate)
    /// — second result set → total row count for pagination metadata
    ///
    /// <c>item_count</c> (snake_case) maps to <c>ItemCount</c> via
    /// <c>DefaultTypeMap.MatchNamesWithUnderscores = true</c>.
    /// </summary>
    public async Task<(IReadOnlyList<OrderSummaryDto> Orders, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = """
            SELECT
                o.public_id,
                o.customer_name,
                o.status,
                o.total_amount,
                o.created_at,
                COUNT(i.id)::int AS item_count
            FROM orders o
            LEFT JOIN order_items i ON i.order_id = o.id
            GROUP BY o.id
            ORDER BY o.created_at DESC
            LIMIT @PageSize OFFSET @Offset;

            SELECT COUNT(*)::int FROM orders;
            """;

        using var multi = await conn.QueryMultipleAsync(
            sql,
            new { PageSize = pageSize, Offset = (page - 1) * pageSize });

        var orders = (await multi.ReadAsync<OrderSummaryDto>()).ToList().AsReadOnly();
        var total  = await multi.ReadSingleAsync<int>();

        return (orders, total);
    }
}
