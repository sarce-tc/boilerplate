using Microservice.Application.Common.Validation;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Models;
using Microservice.Domain.Common;
using Microservice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Microservice.Infrastructure.Repositories.EF;
// ═══════════════════════════════════════════════════════════════════════
// AGENT — EF Core generic repository (all three interfaces in one class).
//
//   IReadRepository<T>  → FindAsync · GetEntityAsync · GetListAsync
//                         GetListPaginatedAsync · ExistsAsync · CountAsync
//   IWriteRepository<T> → AddAsync · Update · UpdateFields · UpdateManyAsync
//                         Delete · DeleteManyAsync
//   IQueryRepository<T> → GetListAsync<TResult>(selector)
//                         GetEntityAsync<TResult>(selector, predicate)
//
//   disableTracking=true (default) → AsNoTracking(); set false when updating
//   UpdateFields → marks only specified properties Modified (PATCH semantics)
//   Changes are staged until IUnitOfWork.SaveChangesAsync() is called
//
// ── GENERIC-FIRST — evaluar en este orden antes de crear métodos específicos ──
//   Obtener entidad por predicado
//     → GetEntityAsync(predicate)
//   Obtener entidad con hijos (eager-loading)
//     → GetEntityAsync(predicate, includeProperties:[e => e.Children])
//   Obtener colección con filtro / orden
//     → GetListAsync(predicate, orderBy)
//   Proyección directa a DTO sin AutoMapper
//     → IQueryRepository<T>.GetListAsync<TResult>(selector, predicate)
//     → IQueryRepository<T>.GetEntityAsync<TResult>(selector, predicate)
//   Colección paginada con metadatos de navegación
//     → GetListPaginatedAsync(currentPage, pageSize, predicate)
//   Verificar existencia / contar
//     → ExistsAsync(predicate) · CountAsync()
//   Bulk delete sin cargar entidades
//     → DeleteManyAsync(predicate)
//   Bulk update sin cargar entidades
//     → UpdateManyAsync(filter, updateAction)
//   Escribir (ADD / UPDATE / DELETE unitario, change-tracked)
//     → IUnitOfWork.ExamplesWrite.AddAsync / Update / UpdateFields / Delete
//   ── Crear método específico SOLO si el caso no cabe en la superficie genérica ──
//   ILike · JOIN complejo · filtro que no existe arriba
//     → Agregar en IMyEntityReadRepository y MyEntityReadRepository
//   Escritura con SQL propio (RETURNING, lógica especial de dominio)
//     → Agregar en IMyEntityWriteRepository y MyEntityWriteRepository
// ═══════════════════════════════════════════════════════════════════════
public class LINQRepository<T>(ExampleDbContext context) :
    ILINQRepository<T>
    where T : BaseDomainModel
{
    protected readonly ExampleDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    #region IReadRepository - LINQ Read Operations

    public async Task<T?> FindAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<T?> GetEntityAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, T>>? select = null,
        IEnumerable<Expression<Func<T, object>>>? includeProperties = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        if (disableTracking)
            query = query.AsNoTracking();

        if (select != null)
            query = query.Select(select);

        if (includeProperties is not null)
        {
            foreach (var includeExpression in includeProperties)
            {
                var path = GetIncludePath(includeExpression);
                if (!string.IsNullOrWhiteSpace(path))
                    query = query.Include(path);
            }
        }

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetListAsync(
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, T>>? select = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        IEnumerable<Expression<Func<T, object>>>? includeProperties = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        if (disableTracking)
            query = query.AsNoTracking();

        if (select != null)
            query = query.Select(select);

        if (includeProperties is not null)
        {
            foreach (var includeExpression in includeProperties)
            {
                var path = GetIncludePath(includeExpression);
                if (!string.IsNullOrWhiteSpace(path))
                    query = query.Include(path);
            }
        }

        if (predicate is not null)
            query = query.Where(predicate);

        if (orderBy is not null)
            query = orderBy(query);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<T>> GetListPaginatedAsync(
        int currentPage,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        IEnumerable<Expression<Func<T, object>>>? includeProperties = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        if (disableTracking)
            query = query.AsNoTracking();

        if (includeProperties is not null)
        {
            foreach (var includeExpression in includeProperties)
            {
                var path = GetIncludePath(includeExpression);
                if (!string.IsNullOrWhiteSpace(path))
                    query = query.Include(path);
            }
        }

        if (predicate is not null)
            query = query.Where(predicate);

        if (orderBy is not null)
            query = orderBy(query);

        var skip = (currentPage - 1) * pageSize;

        var rowsCount = await query.CountAsync(cancellationToken);

        var results = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(results, rowsCount, currentPage, pageSize);
    }

    public async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public Task<int> CountAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbSet.CountAsync(cancellationToken);
    }

    #endregion

    #region IWriteRepository - LINQ Write Operations

    public async Task<T> AddAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public T Update(
        T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        return entity;
    }

    // PATCH semantics: repository.UpdateFields(entity, x => x.Status, x => x.LastModified)
    // generates UPDATE with only specified columns; call SaveChangesAsync() to commit.
    public T UpdateFields(
        T entity,
        Expression<Func<T, object>>[] propertiesToUpdate)
    {
        UpdateFieldValidator.Validate(propertiesToUpdate);

        _context.Attach(entity);

        foreach (var property in propertiesToUpdate)
        {
            _context.Entry(entity).Property(property).IsModified = true;
        }

        return entity;
    }

    public async Task<int> UpdateManyAsync(
        Func<IQueryable<T>, IQueryable<T>> filter,
        Func<IQueryable<T>, Task<int>> updateAction)
    {
        var query = filter(_dbSet);
        return await updateAction(query);
    }

    public void Delete(
        T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<int> DeleteManyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(predicate)
            .ExecuteDeleteAsync(cancellationToken);
    }

    #endregion

    #region IQueryRepository - Projection Operations (DTOs)

    public async Task<IReadOnlyList<TResult>> GetListAsync<TResult>(
        Expression<Func<T, TResult>> select,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _context.Set<T>().AsNoTracking();

        if (predicate is not null)
            query = query.Where(predicate);

        if (orderBy is not null)
            query = orderBy(query);

        return await query.Select(select).ToListAsync(cancellationToken);
    }

    public async Task<TResult?> GetEntityAsync<TResult>(
        Expression<Func<T, TResult>> select,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>()
            .AsNoTracking()
            .Where(predicate)
            .Select(select)
            .FirstOrDefaultAsync(cancellationToken);
    }

    #endregion

    #region Helpers - Include Path Utilities

    private static string GetIncludePath(
        Expression<Func<T, object>> includeExpression)
    {
        Expression body = includeExpression.Body;

        // Maneja el boxing a object: o => (object)o.Prop...
        if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            body = unary.Operand;

        List<string> parts = [];
        VisitExpression(body, parts);
        return string.Join(".", parts);
    }

    private static void VisitExpression(
        Expression expression,
        IList<string> parts)
    {
        switch (expression)
        {
            case MemberExpression member:
                parts.Insert(0, member.Member.Name);
                if (member.Expression != null)
                    VisitExpression(member.Expression, parts);
                break;

            case MethodCallExpression call
                when call.Method.Name == "Select" && call.Arguments.Count == 2:
                {
                    VisitExpression(call.Arguments[0], parts);

                    if (call.Arguments[1] is LambdaExpression lambda)
                        VisitExpression(lambda.Body, parts);
                    break;
                }

            case UnaryExpression unary when unary.NodeType == ExpressionType.Convert:
                VisitExpression(unary.Operand, parts);
                break;

            case ParameterExpression:
            default:
                break;
        }
    }

    #endregion
}
