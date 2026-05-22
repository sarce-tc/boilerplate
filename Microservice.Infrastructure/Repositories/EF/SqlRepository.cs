using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Common;
using Microservice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Microservice.Infrastructure.Repositories.EF
{
    /// <summary>
    /// SQL Repository - Raw SQL and transactional database operations.
    /// 
    /// Architecture Pattern: Data Access Layer for non-LINQ operations
    /// Implements: ISqlQueryRepository, ISqlCommandRepository, ISqlRepository
    /// 
    /// Use Cases:
    /// - Executing complex SQL queries not easily expressible in LINQ
    /// - Running stored procedures for business logic
    /// - Bulk operations requiring performance optimization
    /// - Transactional operations requiring ACID guarantees
    /// - Direct database interactions bypassing the ORM
    /// 
    /// When to Use This Repository:
    /// - When LINQ query translation is insufficient or inefficient
    /// - For complex analytical queries
    /// - When calling pre-existing stored procedures
    /// - For batch operations at scale (bulk insert, update, delete)
    /// - When database-specific features are needed
    /// 
    /// AI Agent Integration Examples:
    /// - FromSqlAsync: Execute complex analytical SQL from AI-generated queries
    /// - ExecuteSqlAsync: Run bulk updates based on AI analysis results
    /// - ExecuteStoredProcedureAsync: Trigger complex business logic procedures
    /// - ExecuteInTransactionAsync: Perform multi-step AI workflows atomically
    /// - ExecuteSqlWithResultAsync: Get results from complex AI analytical queries
    /// 
    /// Security Considerations:
    /// - ALWAYS use FormattableString interpolation ($"..." strings)
    /// - Never concatenate user input or AI-generated strings
    /// - Parameters are automatically parameterized for SQL injection prevention
    /// 
    /// Performance Benefits:
    /// - Direct SQL execution can be 10-100x faster for specific operations
    /// - Avoids ORM overhead for complex queries
    /// - Better suited for aggregate operations and reporting
    /// - Enables use of database-specific optimizations
    /// </summary>
    public class SqlRepository<T>(ExampleDbContext context) :
        ISqlQueryRepository<T>,
        ISqlCommandRepository<T>,
        ISqlRepository<T>
        where T : BaseDomainModel
    {
        protected readonly ExampleDbContext _context = context;
        protected readonly DbSet<T> _dbSet = context.Set<T>();

        /// <summary>
        /// Use Case: Execute a SQL SELECT query and return strongly-typed entities.
        /// 
        /// When to use:
        /// - Executing raw SELECT statements that return multiple rows
        /// - Retrieving results from complex SQL queries
        /// - Fetching data from database views or complex calculations
        /// - AI agents executing SQL queries for data analysis
        /// 
        /// Parameters:
        /// - sql: FormattableString SQL query (e.g., $"SELECT * FROM Examples WHERE Status = {status}")
        /// - cancellationToken: Cancellation support
        /// 
        /// Returns: IReadOnlyList of entities matching the SQL results
        /// 
        /// Security:
        /// - FormattableString automatically parameterizes values
        /// - Prevents SQL injection attacks
        /// 
        /// AI Agent Use Cases:
        /// - Execute complex analytical SQL from AI-generated queries
        /// - Retrieve aggregated data for ML model training
        /// - Fetch filtered datasets for AI processing
        /// 
        /// Example:
        /// var results = await repo.FromSqlAsync(
        ///     $"SELECT * FROM Examples WHERE CreatedDate > {minDate} ORDER BY Score DESC",
        ///     cancellationToken);
        /// </summary>
        public async Task<IReadOnlyList<T>> FromSqlAsync(
            FormattableString sql,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FromSqlInterpolated(sql)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Use Case: Execute a SQL command (INSERT, UPDATE, DELETE) and get affected row count.
        /// 
        /// When to use:
        /// - Running INSERT statements directly
        /// - Executing UPDATE commands with complex logic
        /// - Performing DELETE operations in bulk
        /// - Administrative database maintenance tasks
        /// - Implementing complex business logic in SQL
        /// 
        /// Parameters:
        /// - sql: FormattableString SQL command
        /// - cancellationToken: Cancellation support
        /// 
        /// Returns: Number of rows affected by the command
        /// 
        /// Security:
        /// - FormattableString ensures parameterized execution
        /// - Safe against SQL injection
        /// 
        /// Performance:
        /// - Direct SQL execution, no ORM overhead
        /// - Ideal for bulk operations affecting many rows
        /// 
        /// AI Agent Use Cases:
        /// - Execute bulk updates based on AI analysis
        /// - Perform batch inserts of AI-generated data
        /// - Delete records identified by AI as duplicates
        /// - Execute corrective actions on large datasets
        /// 
        /// Example:
        /// var affected = await repo.ExecuteSqlAsync(
        ///     $"UPDATE Examples SET Status = 'Processed' WHERE ProcessingScore > {threshold}",
        ///     cancellationToken);
        /// 
        /// Important:
        /// - This does NOT use transactions by default
        /// - For transactional safety, use ExecuteInTransactionAsync instead
        /// </summary>
        public async Task<int> ExecuteSqlAsync(
            FormattableString sql,
            CancellationToken cancellationToken = default)
        {
            return await _context.Database
                .ExecuteSqlInterpolatedAsync(sql, cancellationToken);
        }

        /// <summary>
        /// Use Case: Execute a stored procedure in the database.
        /// 
        /// When to use:
        /// - Calling pre-existing stored procedures
        /// - Executing encapsulated business logic in the database
        /// - Running database-specific optimizations
        /// - Integrating with legacy database systems
        /// 
        /// Parameters:
        /// - sql: FormattableString with stored procedure call
        ///   Example: $"EXEC sp_ProcessData @threshold = {threshold}, @processId = {processId}"
        /// 
        /// Returns: Number of rows affected or result from the procedure
        /// 
        /// Stored Procedure Benefits:
        /// - Complex business logic encapsulated in database
        /// - Better performance for complex operations
        /// - Reusable across multiple applications
        /// - Database-level security and auditing
        /// 
        /// AI Agent Use Cases:
        /// - Trigger stored procedures that analyze data
        /// - Execute procedures that apply AI-generated corrections
        /// - Run complex analytical procedures
        /// - Execute maintenance procedures based on AI analysis
        /// 
        /// Example:
        /// var result = await repo.ExecuteStoredProcedureAsync(
        ///     $"EXEC sp_AnalyzeAndUpdateMetrics @startDate = {startDate}, @modelVersion = {modelVersion}",
        ///     cancellationToken);
        /// </summary>
        public async Task<int> ExecuteStoredProcedureAsync(
            FormattableString sql,
            CancellationToken cancellationToken = default)
        {
            return await _context.Database
                .ExecuteSqlInterpolatedAsync(sql, cancellationToken);
        }

        /// <summary>
        /// Use Case: Execute a SQL query and return mapped entities (alternative to FromSqlAsync).
        /// 
        /// When to use:
        /// - Similar to FromSqlAsync, returns entities from SQL query results
        /// - Can be used interchangeably with FromSqlAsync
        /// - Useful for returning results that need entity mapping
        /// 
        /// Parameters:
        /// - sql: FormattableString SQL query
        /// - cancellationToken: Cancellation support
        /// 
        /// Returns: IReadOnlyList of entities
        /// 
        /// Performance: Equivalent to FromSqlAsync
        /// 
        /// Example:
        /// var results = await repo.ExecuteSqlWithResultAsync(
        ///     $"SELECT TOP 100 * FROM Examples WHERE Score > {minScore} ORDER BY CreatedDate DESC",
        ///     cancellationToken);
        /// </summary>
        public async Task<IReadOnlyList<T>> ExecuteSqlWithResultAsync(
            FormattableString sql,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FromSqlInterpolated(sql)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Use Case: Execute multiple SQL operations within a single database transaction.
        /// 
        /// When to use:
        /// - Atomically executing multiple dependent operations
        /// - Ensuring ACID compliance (Atomicity, Consistency, Isolation, Durability)
        /// - Complex workflows where all steps must succeed or all must fail
        /// - Protecting against partial updates or corrupted data
        /// 
        /// Parameters:
        /// - operation: Async function that receives this repository in a transaction context
        /// - cancellationToken: Cancellation support
        /// 
        /// Returns: Result from the operation function
        /// 
        /// Transaction Behavior:
        /// - Automatically begins transaction
        /// - Commits if operation succeeds
        /// - Rolls back entire transaction if exception occurs
        /// - All changes are atomic (all-or-nothing)
        /// 
        /// AI Agent Use Cases:
        /// - Execute multi-step AI workflows atomically
        /// - Create related entities with guaranteed referential integrity
        /// - Apply corrections across multiple records safely
        /// - Ensure data consistency when applying ML-generated suggestions
        /// 
        /// Example:
        /// var result = await repo.ExecuteInTransactionAsync(async (txRepo) =>
        /// {
        ///     // Step 1: Delete old records
        ///     await txRepo.ExecuteSqlAsync(
        ///         $"DELETE FROM Examples WHERE CreatedDate < {cutoffDate}");
        ///     
        ///     // Step 2: Insert new records
        ///     var insertCount = await txRepo.ExecuteSqlAsync(
        ///         $"INSERT INTO Examples (Id, Description) VALUES ({id}, {desc})");
        ///     
        ///     // If either step fails, entire transaction rolls back
        ///     return insertCount;
        /// }, cancellationToken);
        /// 
        /// Important:
        /// - Keep transactions as short as possible
        /// - Long transactions hold locks and reduce concurrency
        /// - Don't use for read-only operations (no benefit, adds overhead)
        /// - Nested transactions are not supported; use savepoints if needed
        /// </summary>
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
}
