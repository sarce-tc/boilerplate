// Microservice.Infrastructure/Repositories/Dapper/ProductReadRepository.cs
using Dapper;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.Entities;

namespace Microservice.Infrastructure.Repositories.Dapper;

public class ProductReadRepository : ReadRepository<Product>, IProductReadRepository
{
    protected override string TableName => "products";
    protected override string SelectColumns =>
        "id, public_id, name, price, created_at, updated_at";

    // Constructor normal — DI
    public ProductReadRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory) { }

    public async Task<IReadOnlyList<Product>> SearchByNameAsync(
        string name, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);
        const string sql = """
            SELECT id, public_id, name, price, created_at, updated_at
            FROM products
            WHERE name ILIKE @Name
            ORDER BY name ASC
            """;
        var result = await conn.QueryAsync<Product>(sql, new { Name = $"%{name}%" });
        return result.ToList().AsReadOnly();
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);
        const string sql = """
            SELECT EXISTS(SELECT 1 FROM products WHERE LOWER(name) = LOWER(@Name))
            """;
        return await conn.ExecuteScalarAsync<bool>(sql, new { Name = name });
    }
}