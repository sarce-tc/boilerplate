using Microservice.Application.Models;
using Microservice.Domain.Common;
using System.Linq.Expressions;

namespace Microservice.Application.Contracts.Persistence.EF;
public interface ILINQRepository<T> : IReadRepository<T>, IWriteRepository<T>, IQueryRepository<T> where T : BaseDomainModel
{
}

/// <summary>
/// Interfaz para operaciones de lectura LINQ
/// .NET 10 + C# 14
/// 
/// ⚠️ NO incluye FromSqlAsync - Para SQL raw, usa ISqlQueryRepository{T}
/// </summary>
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
