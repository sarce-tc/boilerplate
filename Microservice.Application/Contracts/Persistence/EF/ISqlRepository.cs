using Microservice.Domain.Common;

namespace Microservice.Application.Contracts.Persistence.EF;
// Contrato de consulta SQL EF Core — SELECT y stored procedures que solo leen datos.
// FromSqlAsync: ejecuta FormattableString SELECT y retorna entidades mapeadas desde DbSet.
// Usar cuando LINQRepository no es suficiente para expresar la consulta con predicados.
public interface ISqlQueryRepository<T> where T : BaseDomainModel
{
    Task<IReadOnlyList<T>> FromSqlAsync(
        FormattableString sql,
        CancellationToken cancellationToken = default);
}

// Contrato de comando SQL EF Core — INSERT, UPDATE, DELETE y stored procedures que modifican datos.
// ExecuteSqlAsync: ejecuta FormattableString que modifica datos; retorna filas afectadas.
// ExecuteStoredProcedureAsync: llama a stored procedure que modifica datos; retorna filas afectadas.
// ExecuteSqlWithResultAsync: ejecuta UPDATE/INSERT con RETURNING y retorna las entidades resultantes.
// Siempre usar FormattableString ($"...{var}") — EF parameteriza automáticamente para prevenir SQL injection.
public interface ISqlCommandRepository<T> where T : BaseDomainModel
{
    Task<int> ExecuteSqlAsync(
        FormattableString sql,
        CancellationToken cancellationToken = default);

    Task<int> ExecuteStoredProcedureAsync(
        FormattableString sql,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> ExecuteSqlWithResultAsync(
        FormattableString sql,
        CancellationToken cancellationToken = default);
}

// Contrato SQL completo EF Core — combina ISqlQueryRepository<T> e ISqlCommandRepository<T>.
// ExecuteInTransactionAsync: ejecuta múltiples operaciones SQL en una TX explícita con rollback automático;
//   recibe un delegate que recibe el propio ISqlRepository<T> para encadenar reads + writes en la misma TX.
public interface ISqlRepository<T> : ISqlQueryRepository<T>, ISqlCommandRepository<T> where T : BaseDomainModel
{
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<ISqlRepository<T>, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}
