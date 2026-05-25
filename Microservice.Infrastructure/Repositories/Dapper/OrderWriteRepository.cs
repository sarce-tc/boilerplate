using Dapper;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.Entities;
using Npgsql;

namespace Microservice.Infrastructure.Repositories.Dapper;

/// <summary>
/// Dapper write repository for the <c>orders</c> and <c>order_items</c> tables.
///
/// All methods pass <c>_transaction</c> to Dapper so they participate in the
/// UoW transaction opened by <see cref="UnitOfWork.BeginTransactionAsync"/>.
/// Column names use snake_case — mapped to PascalCase properties via
/// <c>DefaultTypeMap.MatchNamesWithUnderscores = true</c>.
/// </summary>
public sealed class OrderWriteRepository : WriteRepository<Order>, IOrderWriteRepository
{
    protected override string TableName => "orders";

    // DI constructor (standalone, no transaction)
    public OrderWriteRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory) { }

    // UoW constructor (shared connection + transaction)
    public OrderWriteRepository(NpgsqlConnection connection, NpgsqlTransaction transaction)
        : base(connection, transaction) { }

    // ── IWriteRepository<Order> ───────────────────────────────────────────────

    public override async Task<Order> AddAsync(Order entity, CancellationToken ct = default)
    {
        var conn = await GetConnectionAsync(ct);
        const string sql = """
            INSERT INTO orders (public_id, customer_name, status, total_amount, created_at, updated_at)
            VALUES (@PublicId, @CustomerName, @Status, @TotalAmount, NOW(), NOW())
            RETURNING id, public_id, customer_name, status, total_amount, created_at, updated_at
            """;
        return await conn.QuerySingleAsync<Order>(sql, entity, _transaction);
    }

    public override async Task<Order> UpdateAsync(Order entity, CancellationToken ct = default)
    {
        var conn = await GetConnectionAsync(ct);
        const string sql = """
            UPDATE orders
            SET status       = @Status,
                total_amount = @TotalAmount,
                updated_at   = NOW()
            WHERE id = @Id
            RETURNING id, public_id, customer_name, status, total_amount, created_at, updated_at
            """;
        return await conn.QuerySingleAsync<Order>(sql, entity, _transaction);
    }

    // ── IOrderWriteRepository ─────────────────────────────────────────────────

    public async Task<OrderItem> AddItemAsync(OrderItem item, CancellationToken ct = default)
    {
        var conn = await GetConnectionAsync(ct);
        const string sql = """
            INSERT INTO order_items
                (public_id, order_id, product_name, quantity, unit_price, line_total, created_at, updated_at)
            VALUES
                (@PublicId, @OrderId, @ProductName, @Quantity, @UnitPrice, @LineTotal, NOW(), NOW())
            RETURNING id, public_id, order_id, product_name, quantity, unit_price, line_total, created_at, updated_at
            """;
        return await conn.QuerySingleAsync<OrderItem>(sql, item, _transaction);
    }
}
