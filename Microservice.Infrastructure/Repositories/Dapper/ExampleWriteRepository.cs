using Dapper;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.Entities;
using Npgsql;
using System.Data;

namespace Microservice.Infrastructure.Repositories.Dapper;
// ═══════════════════════════════════════════════════════════════════════
// AGENT — Repositorio de escritura Dapper del aggregate Example (+ hijos ExampleItem).
// TableName: examples · tabla hija: example_items (FK example_id, ON DELETE CASCADE).
// AddAsync: INSERT del aggregate con RETURNING + INSERT de sus Items (si los hay).
// UpdateAsync: UPDATE escalar del aggregate; NO toca los hijos.
// UpdateWithItemsAsync: UPDATE escalar + replace-all de hijos (DELETE + re-INSERT).
// Todo dentro del TX del UoW (pasar _transaction en cada llamada Dapper).
// ═══════════════════════════════════════════════════════════════════════
public sealed class ExampleWriteRepository : WriteRepository<Example>, IExampleWriteRepository
{
    protected override string TableName => "dapper.examples";

    public ExampleWriteRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory) { }

    public ExampleWriteRepository(NpgsqlConnection connection, NpgsqlTransaction transaction)
        : base(connection, transaction) { }

    public override async Task<Example> AddAsync(Example entity, CancellationToken ct = default)
    {
        var conn = await GetConnectionAsync(ct);
        const string sql = """
        INSERT INTO dapper.examples (public_id, name, description)
        VALUES (@PublicId, @Name, @Description)
        RETURNING id, public_id, name, description, created_at, updated_at
        """;
        var created = await conn.QuerySingleAsync<Example>(sql, entity, _transaction);

        // Hijos: usar el id generado (created.Id) como FK. status/created_at/updated_at
        // los aporta la BD por defecto (mismo criterio que el INSERT del aggregate).
        if (entity.Items.Count > 0)
            await InsertItemsAsync(conn, created.Id, entity.Items, ct);

        return created;
    }

    public override async Task<Example> UpdateAsync(Example entity, CancellationToken ct = default)
    {
        var conn = await GetConnectionAsync(ct);
        const string sql = """
        UPDATE dapper.examples
        SET name        = @Name,
            description = @Description,
            updated_at  = NOW()
        WHERE id = @Id
        RETURNING id, public_id, name, description, created_at, updated_at
        """;
        return await conn.QuerySingleAsync<Example>(sql, entity, _transaction);
    }

    // PATRÓN — replace-all de hijos. Estrategia: borrar todos los example_items del
    // aggregate y re-insertar exactamente entity.Items. entity.Items == [] vacía la
    // colección. Alternativa no implementada: diff/merge por public_id (preserva
    // identidad/timestamps de los items no modificados).
    public async Task<Example> UpdateWithItemsAsync(Example entity, CancellationToken ct = default)
    {
        var updated = await UpdateAsync(entity, ct);

        var conn = await GetConnectionAsync(ct);
        await conn.ExecuteAsync(
            "DELETE FROM dapper.example_items WHERE example_id = @ExampleId",
            new { ExampleId = updated.Id },
            _transaction);

        if (entity.Items.Count > 0)
            await InsertItemsAsync(conn, updated.Id, entity.Items, ct);

        return updated;
    }

    // INSERT por lotes de hijos. Dapper itera la colección de parámetros y ejecuta
    // un INSERT por item, todo en la misma conexión + transacción del UoW.
    private Task InsertItemsAsync(
        IDbConnection conn, int exampleId, IEnumerable<ExampleItem> items, CancellationToken ct)
    {
        // status se escribe EXPLÍCITO desde el dominio (ExampleItemStatus, Pending=1):
        // no depender del default de BD, que duplicaría el valor del dominio y puede
        // derivar (un default 0 sería un enum inválido). created_at/updated_at sí los
        // aporta la BD por defecto, igual que el INSERT del aggregate padre.
        const string sql = """
        INSERT INTO dapper.example_items (example_id, public_id, label, quantity, status)
        VALUES (@ExampleId, @PublicId, @Label, @Quantity, @Status)
        """;
        var rows = items.Select(i => new
        {
            ExampleId = exampleId,
            i.PublicId,
            i.Label,
            i.Quantity,
            Status = (int)i.Status
        });
        return conn.ExecuteAsync(sql, rows, _transaction);
    }
}
