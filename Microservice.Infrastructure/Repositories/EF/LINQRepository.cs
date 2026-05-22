using Microservice.Application.Common.Validation;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Models;
using Microservice.Domain.Common;
using Microservice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Microservice.Infrastructure.Repositories.EF
{
    /// <summary>
    /// LINQ Repository - Multi-purpose Entity Framework repository for read, write, and projection operations.
    /// 
    /// Architecture Pattern: Repository Pattern + Generic Repository
    /// Implements: IReadRepository, IWriteRepository, IQueryRepository (CQRS-friendly separation)
    /// 
    /// Use Cases:
    /// - Standard CRUD operations using Entity Framework LINQ
    /// - Complex queries with filters, includes, and ordering
    /// - Pagination for large result sets
    /// - Batch operations (UpdateMany, DeleteMany)
    /// - Projection queries for DTOs (select specific columns only)
    /// 
    /// When to Use This Repository:
    /// - For typical ORM-based CRUD operations
    /// - When LINQ expression trees provide sufficient query flexibility
    /// - For operations requiring eager loading relationships (Include)
    /// - For AI agents needing to query and manipulate entity data
    /// 
    /// AI Agent Integration Examples:
    /// - FindAsync: Retrieve a specific record for analysis or modification
    /// - GetListAsync with predicates: Filter records based on AI criteria
    /// - GetListPaginatedAsync: Process large datasets in batches
    /// - UpdateFields: Update only specific columns based on AI suggestions
    /// - DeleteManyAsync: Cleanup records identified by AI analysis
    /// 
    /// Performance Considerations:
    /// - AsNoTracking() is default for read operations (better performance)
    /// - Use disableTracking=false only when entity updates are needed
    /// - Pagination prevents memory exhaustion on large result sets
    /// - Projections (Select) reduce data transfer and memory overhead
    /// </summary>
    public class LINQRepository<T>(ExampleDbContext context) :
        IReadRepository<T>,
        IWriteRepository<T>,
        IQueryRepository<T>
        where T : BaseDomainModel
    {
        protected readonly ExampleDbContext _context = context;
        protected readonly DbSet<T> _dbSet = context.Set<T>();

        #region IReadRepository - LINQ Read Operations

        /// <summary>
        /// Use Case: Retrieve a single entity by its primary key (ID).
        /// 
        /// When to use:
        /// - Fetching a specific record by ID from the API
        /// - Validating existence before update/delete operations
        /// - AI agents retrieving a specific entity for analysis
        /// 
        /// Performance: Fast primary key lookup using index
        /// Returns: Entity or null if not found
        /// </summary>
        public async Task<T?> FindAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync([id], cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Use Case: Retrieve a single entity matching complex criteria.
        /// 
        /// Parameters:
        /// - predicate: WHERE clause condition (e.g., x => x.Status == "Active")
        /// - select: Optional projection to specific columns for optimization
        /// - includeProperties: Eagerly load related entities (e.g., x => x.Orders)
        /// - disableTracking: When true, improves read performance; set false if updating
        /// 
        /// When to use:
        /// - Finding by non-ID criteria (e.g., by email, code, or composite keys)
        /// - Loading related data efficiently in a single query
        /// - AI agents searching for entities matching specific conditions
        /// - Complex filtering scenarios
        /// 
        /// Example for AI Agents:
        /// - Find all active records updated since last AI processing
        /// - Retrieve entities matching ML model confidence threshold
        /// - Get records with status requiring AI intervention
        /// </summary>
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

        /// <summary>
        /// Use Case: Retrieve multiple entities matching optional filter criteria.
        /// 
        /// Parameters:
        /// - predicate: Optional WHERE clause for filtering
        /// - select: Optional column selection for projection
        /// - orderBy: Optional custom ordering (e.g., x => x.OrderBy(o => o.CreatedDate))
        /// - includeProperties: Eagerly load related entities
        /// - disableTracking: Read-only optimization flag
        /// 
        /// When to use:
        /// - Fetching all records matching a filter
        /// - Getting sorted data (by date, name, priority, etc.)
        /// - AI agents analyzing collections of entities
        /// - Dashboard queries loading all relevant records
        /// 
        /// AI Agent Use Cases:
        /// - GetListAsync(predicate: x => x.ProcessingStatus == "Pending")
        ///   Retrieve items awaiting AI processing
        /// - GetListAsync(orderBy: x => x.OrderByDescending(o => o.Score))
        ///   Get highest-confidence results from AI analysis
        /// - GetListAsync with includeProperties for context enrichment
        /// 
        /// Note: For large datasets, use GetListPaginatedAsync instead
        /// </summary>
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

        /// <summary>
        /// Use Case: Retrieve paginated results for large datasets.
        /// 
        /// When to use:
        /// - API endpoints with pagination parameters (page, pageSize)
        /// - UI components requiring "load more" or page navigation
        /// - AI agents processing data in manageable batches
        /// - Preventing memory overflow from large result sets
        /// 
        /// Parameters:
        /// - currentPage: 1-based page number
        /// - pageSize: Records per page
        /// - Other parameters same as GetListAsync
        /// 
        /// Returns: PagedResult<T> containing:
        /// - Results: The page of entities
        /// - RowsCount: Total count of all matching records
        /// - CurrentPage: The requested page number
        /// - PageSize: Records per page
        /// 
        /// AI Agent Integration:
        /// - Process large datasets in fixed-size chunks
        /// - Analyze patterns across multiple batches
        /// - Generate reports with pagination support
        /// - Implement streaming analysis without exhausting memory
        /// 
        /// Performance: Optimized with Skip/Take in SQL
        /// </summary>
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

        /// <summary>
        /// Use Case: Check if any entity exists matching a condition.
        /// 
        /// When to use:
        /// - Validation before creating related entities (foreign keys)
        /// - Checking for duplicate records before insertion
        /// - Guard clauses in business logic
        /// - AI agents verifying prerequisites for operations
        /// 
        /// Returns: Boolean (true if exists, false otherwise)
        /// Performance: Efficient EXISTS query, doesn't load full entities
        /// 
        /// Example AI Use Cases:
        /// - Verify a resource exists before creating a reference
        /// - Check if AI-processed data already exists in database
        /// - Validate business rule prerequisites
        /// </summary>
        public async Task<bool> ExistsAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Use Case: Get total count of entities (optionally filtered).
        /// 
        /// When to use:
        /// - Getting total record count for statistics
        /// - Calculating pagination totals
        /// - Monitoring data volume growth
        /// - AI agents analyzing dataset size
        /// 
        /// Returns: Integer count of all entities
        /// Performance: Server-side COUNT query, minimal data transfer
        /// </summary>
        public Task<int> CountAsync(
            CancellationToken cancellationToken = default)
        {
            return _dbSet.CountAsync(cancellationToken);
        }

        #endregion

        #region IWriteRepository - LINQ Write Operations

        /// <summary>
        /// Use Case: Insert a new entity into the database.
        /// 
        /// When to use:
        /// - Creating new records from API requests
        /// - AI agents generating and persisting content
        /// - Bulk import operations
        /// 
        /// Note: Changes are not committed until UnitOfWork.SaveChangesAsync() is called
        /// 
        /// AI Agent Integration:
        /// - Add AI-generated entities to database
        /// - Create suggestion records from ML analysis
        /// - Persist auto-generated content
        /// </summary>
        public async Task<T> AddAsync(
            T entity,
            CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        /// <summary>
        /// Use Case: Update all properties of an existing entity.
        /// 
        /// When to use:
        /// - Full entity updates from API PUT requests
        /// - Replacing entire record with new values
        /// 
        /// Note: Must be called with disableTracking=false for the updated entity
        /// Note: Changes are not committed until UnitOfWork.SaveChangesAsync() is called
        /// </summary>
        public T Update(
            T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        /// <summary>
        /// Use Case: Update only specific properties of an entity.
        /// 
        /// When to use:
        /// - Partial updates (PATCH requests)
        /// - Updating specific fields identified by AI analysis
        /// - Performance optimization (only modified columns in SQL)
        /// - Preserving unmodified fields during update
        /// 
        /// Example:
        /// repository.UpdateFields(entity, x => x.Status, x => x.LastModified);
        /// 
        /// AI Agent Use Cases:
        /// - Update only fields that AI processing identified as needing change
        /// - Selectively apply AI suggestions to specific properties
        /// - Preserve manual edits while applying AI changes to other fields
        /// 
        /// Note: Changes are not committed until UnitOfWork.SaveChangesAsync() is called
        /// </summary>
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

        /// <summary>
        /// Use Case: Update multiple entities in bulk based on a filter.
        /// 
        /// When to use:
        /// - Bulk status updates (e.g., mark many as "processed")
        /// - AI agents updating multiple records based on analysis results
        /// - Applying corrections across related records
        /// - Performance-optimized batch updates
        /// 
        /// Parameters:
        /// - filter: Function to define which records to update
        /// - updateAction: Async function performing the actual update
        /// 
        /// Example:
        /// await repo.UpdateManyAsync(
        ///     q => q.Where(x => x.Status == "Pending"),
        ///     async q => await q.ExecuteUpdateAsync(setters => 
        ///         setters.SetProperty(x => x.Status, "Processed"))
        /// );
        /// 
        /// Performance: Direct SQL UPDATE, doesn't load entities into memory
        /// </summary>
        public async Task<int> UpdateManyAsync(
            Func<IQueryable<T>, IQueryable<T>> filter,
            Func<IQueryable<T>, Task<int>> updateAction)
        {
            var query = filter(_dbSet);
            return await updateAction(query);
        }

        /// <summary>
        /// Use Case: Delete a single entity.
        /// 
        /// When to use:
        /// - Removing records based on user/AI request
        /// - Deleting after finding by ID
        /// 
        /// Note: Changes are not committed until UnitOfWork.SaveChangesAsync() is called
        /// </summary>
        public void Delete(
            T entity)
        {
            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Use Case: Delete multiple entities matching a condition.
        /// 
        /// When to use:
        /// - Bulk deletion operations (e.g., delete all "draft" records)
        /// - Cleanup identified by AI analysis
        /// - Removing expired or obsolete records
        /// - Performance-optimized deletions without loading entities
        /// 
        /// Parameters:
        /// - predicate: Condition for records to delete (e.g., x => x.Status == "Obsolete")
        /// 
        /// Returns: Number of entities deleted
        /// 
        /// AI Agent Use Cases:
        /// - Delete records identified as duplicates by AI
        /// - Remove low-quality or irrelevant generated content
        /// - Clean up based on ML analysis results
        /// 
        /// Performance: Direct SQL DELETE, doesn't load entities into memory
        /// </summary>
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

        /// <summary>
        /// Use Case: Retrieve a list of entities projected to a different type (DTO).
        /// 
        /// When to use:
        /// - Mapping entities to DTOs without loading full entities
        /// - Selecting specific columns from database (performance optimization)
        /// - Creating API response objects efficiently
        /// - Reducing memory footprint by selecting only needed data
        /// 
        /// Parameters:
        /// - select: Projection expression (e.g., x => new MyDto { Id = x.Id, Name = x.Name })
        /// - predicate: Optional filter condition
        /// - orderBy: Optional custom ordering
        /// 
        /// Performance Benefits:
        /// - SQL SELECT includes only the columns in the projection
        /// - Reduces network data transfer
        /// - Minimizes object instantiation overhead
        /// 
        /// AI Agent Use Cases:
        /// - Project only fields relevant to AI analysis
        /// - Retrieve optimized DTO structure for external AI services
        /// - Create custom views for specific AI workflows
        /// </summary>
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

        /// <summary>
        /// Use Case: Retrieve a single entity projected to a different type.
        /// 
        /// When to use:
        /// - Fetching a specific record as DTO by ID or other criteria
        /// - Selecting specific columns for detailed view
        /// - API endpoints returning single projected resources
        /// 
        /// Parameters:
        /// - select: Projection expression to target DTO type
        /// - predicate: Filter condition (e.g., x => x.Id == id)
        /// 
        /// Returns: Projected entity or null if not found
        /// 
        /// Performance: Minimal data transfer, only selected columns
        /// </summary>
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

            var parts = new List<string>();
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
}