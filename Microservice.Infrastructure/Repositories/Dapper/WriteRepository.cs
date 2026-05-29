using Dapper;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.ValueObjects;
using Npgsql;
using System.Data;

namespace Microservice.Infrastructure.Repositories.Dapper;
// ═══════════════════════════════════════════════════════════════════════
// AGENT — Dapper write base. Subclass and override TableName, AddAsync, UpdateAsync.
// Two constructors — always implement both in concrete repos:
//   DI  → WriteRepository(IDbConnectionFactory)                standalone use
//   UoW → WriteRepository(NpgsqlConnection, NpgsqlTransaction)  inside UnitOfWork
// DeleteAsync is fully implemented here (DELETE FROM {table} WHERE id = @Id).
// Pass _transaction in every Dapper call so the SQL joins the active UoW TX.
//
// ── GENERIC-FIRST — evaluar en este orden antes de crear métodos específicos ──
//   Insertar un registro                   → AddAsync(entity)      [override obligatorio]
//   Actualizar un registro                 → UpdateAsync(entity)   [override obligatorio]
//   Eliminar un registro por id interno    → DeleteAsync(id)       [implementado en base]
//   ── Agregar método en la subclase SOLO si el caso no cabe arriba ──
//   INSERT/UPDATE con columnas específicas · RETURNING · lógica de dominio propia
//     → Agregar en IMyEntityWriteRepository e implementar en MyEntityWriteRepository
//   Todas las escrituras deben ir dentro del bloque IUnitOfWork (BeginTransaction /
//   Commit / Rollback) para garantizar atomicidad — nunca inyectar WriteRepository
//   directamente en un handler; usar siempre IUnitOfWork.ExamplesWrite
// ═══════════════════════════════════════════════════════════════════════
public abstract class WriteRepository<T> : IWriteRepository<T>
    where T : BaseDomainModel
{
    private readonly IDbConnectionFactory? _connectionFactory;
    protected readonly IDbConnection? _externalConnection;
    protected IDbTransaction? _transaction;

    protected WriteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    protected WriteRepository(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        _externalConnection = connection;
        _transaction = transaction;
    }

    protected async Task<IDbConnection> GetConnectionAsync(CancellationToken ct)
    {
        if (_externalConnection is not null) return _externalConnection;
        return await _connectionFactory!.CreateAsync(ct);
    }

    public abstract Task<T> AddAsync(T entity, CancellationToken ct = default);
    public abstract Task<T> UpdateAsync(T entity, CancellationToken ct = default);

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var conn = await GetConnectionAsync(ct);
        await conn.ExecuteAsync(
            $"DELETE FROM {TableName} WHERE id = @Id",
            new { Id = id },
            _transaction);  // ← pasa la transacción si existe
    }

    protected abstract string TableName { get; }
}
