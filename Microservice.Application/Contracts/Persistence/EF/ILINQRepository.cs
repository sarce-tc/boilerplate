using Microservice.Application.Models;
using Microservice.Domain.Common;
using System.Linq.Expressions;

namespace Microservice.Application.Contracts.Persistence.EF;
// Interfaz unificada EF Core — agrupa IReadRepository<T>, IWriteRepository<T> e IQueryRepository<T>.
// Implementada por LINQRepository<T>; registrar en DI con los tres contratos apuntando a la misma instancia.
public interface ILINQRepository<T> : IReadRepository<T>, IWriteRepository<T>, IQueryRepository<T> where T : BaseDomainModel
{
}

// Contrato de lectura LINQ EF Core para cualquier aggregate.
// FindAsync: busca por id interno (PK) usando DbSet.FindAsync.
// GetEntityAsync: busca por predicado lambda con eager-loading opcional (includeProperties) y tracking configurable.
// GetListAsync: colección filtrada, ordenada y con eager-loading opcional; disableTracking=true por defecto.
// GetListPaginatedAsync: emite COUNT(*) + SELECT LIMIT/OFFSET con predicado y orden; retorna PagedResult<T>.
// ExistsAsync: AnyAsync con predicado — sin cargar entidades.
// CountAsync: total de registros de la tabla.
public interface IReadRepository<T> where T : BaseDomainModel
{
    Task<T?> FindAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<T?> GetEntityAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, T>>? select = null,
        IEnumerable<Expression<Func<T, object>>>? includeProperties = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetListAsync(
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, T>>? select = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        IEnumerable<Expression<Func<T, object>>>? includeProperties = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default);

    Task<PagedResult<T>> GetListPaginatedAsync(
        int currentPage,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        IEnumerable<Expression<Func<T, object>>>? includeProperties = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        CancellationToken cancellationToken = default);
}

// Contrato de escritura LINQ EF Core para cualquier aggregate.
// AddAsync: agrega la entidad al DbSet (staged) sin confirmar.
// Update: marca la entidad como Modified para que EF genere UPDATE al hacer SaveChanges.
// UpdateFields: PATCH — marca solo las propiedades especificadas como Modified.
// UpdateManyAsync: bulk update sin cargar entidades via ExecuteUpdateAsync.
// Delete: quita la entidad del DbSet (staged).
// DeleteManyAsync: bulk delete via ExecuteDeleteAsync sin cargar entidades.
// Los cambios se confirman con IUnitOfWork.SaveChangesAsync() — sin TX implícita propia.
public interface IWriteRepository<T> where T : BaseDomainModel
{
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    T Update(T entity);
    T UpdateFields(
        T entity,
        Expression<Func<T, object>>[] propertiesToUpdate);
    Task<int> UpdateManyAsync(
        Func<IQueryable<T>, IQueryable<T>> filter,
        Func<IQueryable<T>, Task<int>> updateAction);
    void Delete(T entity);
    Task<int> DeleteManyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);
}

// Contrato de proyección EF Core — SELECT directo a TResult sin pasar por AutoMapper.
// GetListAsync<TResult>: colección proyectada con selector lambda + filtro y orden opcionales.
// GetEntityAsync<TResult>: entidad única proyectada con selector + predicado obligatorio.
public interface IQueryRepository<T>
    where T : BaseDomainModel
{
    Task<IReadOnlyList<TResult>> GetListAsync<TResult>(
        Expression<Func<T, TResult>> select,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default);

    Task<TResult?> GetEntityAsync<TResult>(
        Expression<Func<T, TResult>> select,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);
}
