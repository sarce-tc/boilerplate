using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Commands.ExecuteSql
{
    /// <summary>
    /// Use Case: Execute raw SQL commands or stored procedures for advanced database operations.
    /// 
    /// When to use:
    /// - When complex operations require raw SQL performance optimization
    /// - For executing stored procedures with specific business logic
    /// - When bulk operations need direct SQL execution
    /// - For administrative tasks that bypass ORM for efficiency
    /// 
    /// Responsibilities:
    /// - Execute parameterized SQL commands safely (using FormattableString)
    /// - Return the number of affected rows
    /// - Maintain database integrity through proper SQL practices
    /// 
    /// AI Agent Use Cases:
    /// - AI systems can use this to:
    ///   * Execute complex analytical queries
    ///   * Perform batch updates based on AI-generated logic
    ///   * Run maintenance operations on large datasets
    ///   * Execute domain-specific stored procedures for analysis
    /// 
    /// Security Considerations:
    /// - ALWAYS use FormattableString interpolation to prevent SQL injection
    /// - Never concatenate user input directly into SQL
    /// - Validate and sanitize parameters before execution
    /// 
    /// Performance:
    /// - Direct SQL execution can be faster than LINQ for specific scenarios
    /// - Use when ORM overhead is a bottleneck
    /// </summary>
    public class ExecuteSqlCommandHandler(
        ISqlCommandRepository<Example> sqlCommandRepository
        ) : IRequestHandler<ExecuteSqlCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(ExecuteSqlCommand request, CancellationToken cancellationToken)
        {
            var result = await sqlCommandRepository.ExecuteSqlAsync(request.Sql, cancellationToken);
            return Result<int>.Success(result);
        }
    }
}
