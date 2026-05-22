using Microservice.Domain.Common;

namespace Microservice.Application.Contracts.Persistence.EF;

/// <summary>
/// Repositorio para operaciones SQL que CONSULTAN datos (SELECT)
/// Solo lectura - NUNCA modifica datos
/// .NET 10 + C# 14
/// 
/// ✅ Para: SELECT, Stored Procedures que retornan datos sin modificar
/// ❌ NO para: INSERT, UPDATE, DELETE
/// </summary>
public interface ISqlQueryRepository<T> where T : BaseDomainModel
{
    /// <summary>
    /// Ejecuta SELECT SQL raw directamente
    /// ✅ GARANTIZADO: Solo lectura, no modifica datos
    /// 
    /// Ejemplos:
    /// await sqlQueryRepo.FromSqlAsync($"SELECT * FROM Products WHERE Price > {price}");
    /// await sqlQueryRepo.FromSqlAsync($"SELECT TOP 10 * FROM Orders ORDER BY CreatedAt DESC");
    /// </summary>
    Task<IReadOnlyList<T>> FromSqlAsync(
        FormattableString sql,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repositorio para operaciones SQL que MODIFICAN datos (INSERT, UPDATE, DELETE)
/// AQUÍ van ExecuteSqlRaw / ExecuteSqlInterpolated cuando modifican
/// .NET 10 + C# 14
/// 
/// ✅ Para: INSERT, UPDATE, DELETE directo, EXEC sp_Procedure
/// ❌ NO para: SELECT puro
/// </summary>
public interface ISqlCommandRepository<T> where T : BaseDomainModel
{
    /// <summary>
    /// Ejecuta comando SQL raw que MODIFICA datos
    /// Para: INSERT, UPDATE, DELETE directo
    /// Retorna: Número de registros afectados
    /// 
    /// Ejemplos:
    /// var affected = await sqlCommandRepo.ExecuteSqlAsync(
    ///     $"UPDATE Products SET Price = {newPrice} WHERE Category = {category}");
    /// 
    /// var deleted = await sqlCommandRepo.ExecuteSqlAsync(
    ///     $"DELETE FROM Orders WHERE CreatedAt < {cutoffDate}");
    /// 
    /// var inserted = await sqlCommandRepo.ExecuteSqlAsync(
    ///     $"INSERT INTO AuditLog VALUES ({userId}, {action}, {timestamp})");
    /// </summary>
    Task<int> ExecuteSqlAsync(
        FormattableString sql,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta un stored procedure que MODIFICA datos
    /// Para: EXEC sp_InsertData, EXEC sp_BatchUpdate, EXEC sp_Archive, etc
    /// Retorna: Número de registros afectados
    /// 
    /// Ejemplos:
    /// var affected = await sqlCommandRepo.ExecuteStoredProcedureAsync(
    ///     $"EXEC sp_ArchiveOldOrders @Days = {days}");
    /// 
    /// var processed = await sqlCommandRepo.ExecuteStoredProcedureAsync(
    ///     $"EXEC sp_ProcessPendingPayments @BatchSize = {batchSize}");
    /// </summary>
    Task<int> ExecuteStoredProcedureAsync(
        FormattableString sql,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta comando SQL que MODIFICA y retorna los registros resultantes
    /// Para: UPDATE...RETURNING (PostgreSQL) o OUTPUT (SQL Server)
    /// Retorna: Los registros modificados/insertados
    /// 
    /// Ejemplos (PostgreSQL):
    /// var updated = await sqlCommandRepo.ExecuteSqlWithResultAsync(
    ///     $@"UPDATE Products 
    ///        SET Price = Price * {factor}
    ///        WHERE Category = {category}
    ///        RETURNING *");
    /// </summary>
    Task<IReadOnlyList<T>> ExecuteSqlWithResultAsync(
        FormattableString sql,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repositorio SQL completo - lectura Y escritura en una transacción
/// Para operaciones transaccionales complejas
/// .NET 10 + C# 14
/// 
/// ✅ Para: Múltiples operaciones en transacción con rollback automático
/// </summary>
public interface ISqlRepository<T> : ISqlQueryRepository<T>, ISqlCommandRepository<T> where T : BaseDomainModel
{
    /// <summary>
    /// Ejecutar múltiples operaciones SQL en una TRANSACCIÓN
    /// Si algo falla → TODO se revierte automáticamente ✅
    /// 
    /// Ejemplo:
    /// var result = await sqlRepository.ExecuteInTransactionAsync(async repo =>
    /// {
    ///     var items = await repo.FromSqlAsync($"SELECT...");
    ///     foreach (var item in items)
    ///     {
    ///         await repo.ExecuteSqlAsync($"UPDATE...");
    ///     }
    ///     return items.Count;
    /// });
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<ISqlRepository<T>, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}
