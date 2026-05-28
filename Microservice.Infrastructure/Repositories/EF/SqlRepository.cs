using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Common;
using Microservice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Microservice.Infrastructure.Repositories.EF;
// ═══════════════════════════════════════════════════════════════════════
// AGENT — EF Core raw-SQL repository for operations beyond LINQ.
//
//   ISqlQueryRepository<T>  → FromSqlAsync · ExecuteSqlWithResultAsync
//   ISqlCommandRepository<T>→ ExecuteSqlAsync · ExecuteStoredProcedureAsync
//   ISqlRepository<T>       → all of the above + ExecuteInTransactionAsync
//
//   Always use FormattableString ($"...{var}") — EF parameterizes automatically.
// ═══════════════════════════════════════════════════════════════════════
public sealed class SqlRepository<T>(ExampleDbContext context) :
    ISqlQueryRepository<T>,
    ISqlCommandRepository<T>,
    ISqlRepository<T>
    where T : BaseDomainModel
{
    protected readonly ExampleDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<IReadOnlyList<T>> FromSqlAsync(
        FormattableString sql,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FromSqlInterpolated(sql)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ExecuteSqlAsync(
        FormattableString sql,
        CancellationToken cancellationToken = default)
    {
        return await _context.Database
            .ExecuteSqlInterpolatedAsync(sql, cancellationToken);
    }

    public async Task<int> ExecuteStoredProcedureAsync(
        FormattableString sql,
        CancellationToken cancellationToken = default)
    {
        return await _context.Database
            .ExecuteSqlInterpolatedAsync(sql, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> ExecuteSqlWithResultAsync(
        FormattableString sql,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FromSqlInterpolated(sql)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<ISqlRepository<T>, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database
            .BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await operation(this);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
