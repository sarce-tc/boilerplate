using Dapper;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.Entities;
using Npgsql;

namespace Microservice.Infrastructure.Repositories.Dapper;

public sealed class CustomerWriteRepository : WriteRepository<Customer>, ICustomerWriteRepository
{
    protected override string TableName => "customers";

    // DI constructor (standalone, no transaction)
    public CustomerWriteRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory) { }

    // UoW constructor (shared connection + transaction)
    public CustomerWriteRepository(NpgsqlConnection connection, NpgsqlTransaction transaction)
        : base(connection, transaction) { }

    public override async Task<Customer> AddAsync(Customer entity, CancellationToken ct = default)
    {
        var conn = await GetConnectionAsync(ct);
        const string sql = """
            INSERT INTO customers (public_id, name, email, phone, created_at, updated_at)
            VALUES (@PublicId, @Name, @Email, @Phone, NOW(), NOW())
            RETURNING id, public_id, name, email, phone, created_at, updated_at
            """;
        return await conn.QuerySingleAsync<Customer>(sql, entity, _transaction);
    }

    public override async Task<Customer> UpdateAsync(Customer entity, CancellationToken ct = default)
    {
        var conn = await GetConnectionAsync(ct);
        const string sql = """
            UPDATE customers
            SET name       = @Name,
                email      = @Email,
                phone      = @Phone,
                updated_at = NOW()
            WHERE id = @Id
            RETURNING id, public_id, name, email, phone, created_at, updated_at
            """;
        return await conn.QuerySingleAsync<Customer>(sql, entity, _transaction);
    }
}
