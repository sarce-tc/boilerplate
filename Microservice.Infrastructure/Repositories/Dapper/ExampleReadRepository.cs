using Dapper;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;
using Microservice.Application.Models;
using Microservice.Domain.Entities;
using Npgsql;
using System.Data;

namespace Microservice.Infrastructure.Repositories.Dapper;
// ═══════════════════════════════════════════════════════════════════════
// AGENT — Repositorio de lectura Dapper del aggregate Example.
// TableName: dapper.examples · tabla hija: dapper.example_items
// SearchByNameAsync / ExistsByNameAsync: operan sobre el aggregate plano (sin hijos).
//
// PATRÓN — Reads con hijos (*WithItemsAsync): proyectan DTOs por JOIN + multi-mapping,
// NO hidratan el aggregate de dominio (Example.Items es read-only y solo se llena por
// AddItem, que regenera identidad). La agrupación padre→hijos se hace en memoria por id.
// ═══════════════════════════════════════════════════════════════════════
public sealed class ExampleReadRepository
    : ReadRepository<Example>, IExampleReadRepository
{
    protected override string TableName => "dapper.examples";
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
        FROM dapper.examples
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
        SELECT EXISTS(SELECT 1 FROM dapper.examples WHERE LOWER(name) = LOWER(@Name))
        """;
        return await conn.ExecuteScalarAsync<bool>(sql, new { Name = name });
    }

    // ── Reads con hijos (JOIN + multi-mapping) ───────────────────────────────

    // Columnas: primero el padre, luego el hijo. splitOn:"item_id" separa ambos objetos.
    private const string WithItemsColumns = """
        e.id, e.public_id, e.name, e.description, e.created_at, e.updated_at,
        i.id AS item_id, i.public_id, i.label, i.quantity, i.status, i.created_at, i.updated_at
        """;

    public async Task<GetExampleByPublicIdDto?> GetByPublicIdWithItemsAsync(
        Guid publicId, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);
        var sql = $"""
        SELECT {WithItemsColumns}
        FROM dapper.examples e
        LEFT JOIN dapper.example_items i ON i.example_id = e.id
        WHERE e.public_id = @PublicId
        ORDER BY i.id
        """;
        var rows = await QueryWithItemsAsync(conn, sql, new { PublicId = publicId },
            (e, items) => new GetExampleByPublicIdDto(
                e.PublicId, e.Name, e.Description, e.CreatedAt, e.UpdatedAt, items));
        return rows.SingleOrDefault();
    }

    public async Task<PagedResult<GetExamplesPaginatedDto>> GetPaginatedWithItemsAsync(
        int currentPage, int pageSize, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        var rowsCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM dapper.examples");

        var skip = (currentPage - 1) * pageSize;
        // LIMIT/OFFSET sobre el PADRE (subconsulta) y luego JOIN de hijos de esa página.
        var sql = $"""
        SELECT {WithItemsColumns}
        FROM (
            SELECT * FROM dapper.examples ORDER BY id LIMIT @PageSize OFFSET @Skip
        ) e
        LEFT JOIN dapper.example_items i ON i.example_id = e.id
        ORDER BY e.id, i.id
        """;
        var results = await QueryWithItemsAsync(conn, sql, new { PageSize = pageSize, Skip = skip },
            (e, items) => new GetExamplesPaginatedDto(
                e.PublicId, e.Name, e.Description, e.CreatedAt, e.UpdatedAt, items));

        return new PagedResult<GetExamplesPaginatedDto>(results, rowsCount, currentPage, pageSize);
    }

    public async Task<IReadOnlyList<GetAllExamplesDto>> GetAllWithItemsAsync(CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);
        var sql = $"""
        SELECT {WithItemsColumns}
        FROM dapper.examples e
        LEFT JOIN dapper.example_items i ON i.example_id = e.id
        ORDER BY e.id, i.id
        """;
        return await QueryWithItemsAsync(conn, sql, param: null,
            (e, items) => new GetAllExamplesDto(
                e.PublicId, e.Name, e.Description, e.CreatedAt, e.UpdatedAt, items));
    }

    public async Task<IReadOnlyList<SearchExamplesByNameDto>> SearchByNameWithItemsAsync(
        string name, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);
        var sql = $"""
        SELECT {WithItemsColumns}
        FROM dapper.examples e
        LEFT JOIN dapper.example_items i ON i.example_id = e.id
        WHERE e.name ILIKE @Name
        ORDER BY e.name ASC, i.id
        """;
        return await QueryWithItemsAsync(conn, sql, new { Name = $"%{name}%" },
            (e, items) => new SearchExamplesByNameDto(
                e.PublicId, e.Name, e.Description, e.CreatedAt, e.UpdatedAt, items));
    }

    // Multi-mapping genérico: agrupa filas padre+hijo por id de padre (preservando orden)
    // y proyecta cada grupo al DTO destino vía factory.
    private async Task<List<TDto>> QueryWithItemsAsync<TDto>(
        IDbConnection conn,
        string sql,
        object? param,
        Func<ExampleRow, IReadOnlyList<GetExampleItemDto>, TDto> factory)
    {
        var byId = new Dictionary<int, (ExampleRow Parent, List<GetExampleItemDto> Items)>();
        var order = new List<int>();

        await conn.QueryAsync<ExampleRow, ItemRow, ExampleRow>(
            sql,
            (e, i) =>
            {
                if (!byId.TryGetValue(e.Id, out var acc))
                {
                    acc = (e, []);
                    byId[e.Id] = acc;
                    order.Add(e.Id);
                }
                if (i is not null && i.ItemId != 0)
                    acc.Items.Add(new GetExampleItemDto(
                        i.PublicId, i.Label, i.Quantity,
                        (ExampleItemStatus)i.Status, i.CreatedAt, i.UpdatedAt));
                return e;
            },
            param,
            _transaction,
            splitOn: "item_id");

        return order.Select(id => factory(byId[id].Parent, byId[id].Items)).ToList();
    }

    // POCOs de multi-mapping (mutables, hidratados por Dapper con underscore-matching).
    private sealed class ExampleRow
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    private sealed class ItemRow
    {
        public int ItemId { get; set; }    // alias i.id AS item_id (evita choque con e.id)
        public Guid PublicId { get; set; }
        public string Label { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
