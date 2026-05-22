// Microservice.Infrastructure/Repositories/Dapper/ProductWriteRepository.cs
using Dapper;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.Entities;
using Npgsql;

namespace Microservice.Infrastructure.Repositories.Dapper;

public class ProductWriteRepository : WriteRepository<Product>, IProductWriteRepository
{
    protected override string TableName => "products";

    // Constructor normal — DI
    public ProductWriteRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory) { }

    // Constructor para UoW
    public ProductWriteRepository(NpgsqlConnection connection, NpgsqlTransaction transaction)
        : base(connection, transaction) { }

    public override async Task<Product> AddAsync(Product entity, CancellationToken ct = default)
    {
        var conn = await GetConnectionAsync(ct);
        const string sql = """
            INSERT INTO products (public_id, name, price)
            VALUES (@PublicId, @Name, @Price)
            RETURNING id, public_id, name, price, created_at, updated_at
            """;
        return await conn.QuerySingleAsync<Product>(sql, entity, _transaction);
    }

    public override async Task<Product> UpdateAsync(Product entity, CancellationToken ct = default)
    {
        var conn = await GetConnectionAsync(ct);
        const string sql = """
            UPDATE products
            SET name       = @Name,
                price      = @Price,
                updated_at = NOW()
            WHERE id = @Id
            RETURNING id, public_id, name, price, created_at, updated_at
            """;
        return await conn.QuerySingleAsync<Product>(sql, entity, _transaction);
    }
}