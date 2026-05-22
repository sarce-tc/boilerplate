using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.ExecuteSqlWithResult
{
    /// <summary>
    /// Use Case: Execute a raw SQL query and return results as strongly-typed entities.
    /// 
    /// When to use:
    /// - Executing SQL queries passed from API request
    /// - Dynamic SQL execution based on user/AI-specified queries
    /// - Complex queries that LINQ cannot express
    /// - Stored procedure result retrieval
    /// 
    /// Responsibilities:
    /// - Accept SQL query from request
    /// - Execute via ISqlQueryRepository
    /// - Map results to DTO collection
    /// - Return results wrapped in Result<T>
    /// 
    /// Difference from GetExamplesFromSqlQueryHandler:
    /// - GetExamplesFromSql: Handler constructs the SQL query
    /// - ExecuteSqlWithResult: Query is passed in from request (more flexible)
    /// 
    /// Security Considerations:
    /// - Request.Sql should already be a FormattableString (parameterized)
    /// - Additional validation may be needed for dynamic SQL from user input
    /// - Consider implementing query whitelisting for sensitive systems
    /// 
    /// AI Agent Integration:
    /// - AI agents can generate SQL queries and execute them dynamically
    /// - Useful for flexible data exploration by AI systems
    /// - Enables agents to construct analysis queries at runtime
    /// - Supports dynamic filtering based on AI analysis results
    /// 
    /// Returns: Collection of DTOs mapped from SQL results
    /// 
    /// Note:
    /// - Validate SQL queries if accepting from untrusted sources
    /// - Consider query complexity limits to prevent resource exhaustion
    /// - Monitor execution time for performance
    /// </summary>
    public class ExecuteSqlWithResultQueryHandler(
        ISqlQueryRepository<Example> sqlQueryRepository,
        IMapper mapper
        ) : IRequestHandler<ExecuteSqlWithResultQuery, Result<IReadOnlyList<ExecuteSqlWithResultDto>>>
    {
        public async Task<Result<IReadOnlyList<ExecuteSqlWithResultDto>>> Handle(ExecuteSqlWithResultQuery request, CancellationToken cancellationToken)
        {
            var examples = await sqlQueryRepository.FromSqlAsync(request.Sql, cancellationToken);
            var data = mapper.Map<IReadOnlyList<ExecuteSqlWithResultDto>>(examples);
            return Result<IReadOnlyList<ExecuteSqlWithResultDto>>.Success(data);
        }
    }
}
