using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.GetExamplesFromSql
{
    /// <summary>
    /// Use Case: Execute raw SQL SELECT queries for complex data retrieval.
    /// 
    /// When to use:
    /// - Complex queries with complex filtering, joins, aggregations
    /// - Executing database-specific SQL optimizations
    /// - Reporting queries that LINQ cannot easily express
    /// - Complex analytical queries for AI-driven analysis
    /// - Queries requiring specific SQL functions (window functions, CTEs, etc.)
    /// 
    /// Responsibilities:
    /// - Construct parameterized SQL query using FormattableString
    /// - Execute raw SQL via ISqlQueryRepository
    /// - Map results to DTOs
    /// - Return results wrapped in Result<T>
    /// 
    /// Architecture:
    /// - Uses ISqlQueryRepository for direct SQL SELECT execution
    /// - More efficient than LINQ for complex reporting scenarios
    /// - Bypasses ORM overhead while maintaining safety
    /// 
    /// Security Considerations:
    /// - ALWAYS use FormattableString ($"...") for parameter interpolation
    /// - Parameterization is automatic and prevents SQL injection
    /// - Never concatenate user input directly into SQL strings
    /// 
    /// Performance Benefits:
    /// - Direct SQL execution without ORM translation overhead
    /// - Can leverage database-specific optimizations
    /// - Efficient for large result sets
    /// - Better query plans for complex scenarios
    /// 
    /// AI Agent Use Cases:
    /// - Execute complex analytical SQL for data mining
    /// - Retrieve aggregated data for ML model training
    /// - Run complex filtering for data preparation
    /// - Execute windowing functions for time-series analysis
    /// - Perform complex joins for enriched data analysis
    /// 
    /// Example SQL Queries:
    /// - Complex aggregations: $"SELECT Status, COUNT(*) as Count FROM Examples GROUP BY Status"
    /// - Window functions: $"SELECT Id, ROW_NUMBER() OVER (ORDER BY Score DESC) FROM Examples"
    /// - Complex filtering: $"SELECT * FROM Examples WHERE Created > {startDate} AND Score > {threshold}"
    /// 
    /// .NET 10 + C# 14 Features:
    /// - FormattableString interpolation for safe parameterization
    /// - async/await for non-blocking queries
    /// - Record types for immutable DTOs
    /// </summary>
    public class GetExamplesFromSqlQueryHandler(
        ISqlQueryRepository<Example> sqlQueryRepository,
        IMapper mapper
        ) : IRequestHandler<GetExamplesFromSqlQuery, Result<IEnumerable<GetExamplesFromSqlDto>>>
    {
        public async Task<Result<IEnumerable<GetExamplesFromSqlDto>>> Handle(
            GetExamplesFromSqlQuery request, 
            CancellationToken cancellationToken)
        {
            // ✅ Use ISqlQueryRepository for raw SELECT queries
            // This is more efficient for complex reports and aggregations
            FormattableString sql = $"SELECT * FROM \"Examples\" WHERE \"Id\" > 0";

            var examples = await sqlQueryRepository.FromSqlAsync(sql, cancellationToken);
            var data = mapper.Map<IEnumerable<GetExamplesFromSqlDto>>(examples);

            return Result<IEnumerable<GetExamplesFromSqlDto>>.Success(data);
        }
    }
}
