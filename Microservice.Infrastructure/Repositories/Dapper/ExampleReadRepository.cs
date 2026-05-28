using Dapper;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.Entities;
using Npgsql;

namespace Microservice.Infrastructure.Repositories.Dapper;
// ═══════════════════════════════════════════════════════════════════════
// AGENT — Repositorio de lectura Dapper del aggregate Example.
// TableName: examples
// SearchByNameAsync: búsqueda ILIKE por nombre con wildcard, ordena por nombre ASC.
// ExistsByNameAsync: comprobación de unicidad case-insensitive via LOWER() sin cargar entidad.
// ═══════════════════════════════════════════════════════════════════════
public sealed class ExampleReadRepository
    : ReadRepository<Example>, IExampleReadRepository
{
    protected override string TableName => "examples";
    protected override string SelectColumns =>
        "id, public_id, name, description, created_at, updated_at";

    public ExampleReadRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory) { }

    public ExampleReadRepository(NpgsqlConnection connection, NpgsqlTransaction transaction)
        : base(connection, transaction) { }

    public async Task<IReadOnlyList<Example>> SearchByNameAsync(
        string name, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);
        const string sql = """
        SELECT id, public_id, name, description, created_at, updated_at
        FROM examples
        WHERE name ILIKE @Name
        ORDER BY name ASC
        """;
        var result = await conn.QueryAsync<Example>(sql, new { Name = $"%{name}%" });
        return result.ToList().AsReadOnly();
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);
        const string sql = """
        SELECT EXISTS(SELECT 1 FROM examples WHERE LOWER(name) = LOWER(@Name))
        """;
        return await conn.ExecuteScalarAsync<bool>(sql, new { Name = name });
    }
}
