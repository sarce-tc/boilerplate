using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Commands.ExecuteStoredProcedure
{
    /// <summary>
    /// Use Case: Execute pre-defined stored procedures in the database.
    /// 
    /// When to use:
    /// - When complex business logic is encapsulated in stored procedures
    /// - For executing database-specific optimizations
    /// - When integrating with legacy database systems
    /// - For administrative maintenance operations
    /// 
    /// Responsibilities:
    /// - Execute the specified stored procedure by name
    /// - Pass parameters securely using FormattableString
    /// - Return the number of affected rows
    /// - Maintain parameter type safety and SQL injection prevention
    /// 
    /// AI Agent Use Cases:
    /// - AI systems can use this to:
    ///   * Execute complex analytical stored procedures
    ///   * Trigger database maintenance operations
    ///   * Call domain-specific business logic procedures
    ///   * Execute reporting procedures for data analysis
    /// 
    /// Benefits of Stored Procedures:
    /// - Encapsulation of complex SQL logic
    /// - Better performance for complex operations
    /// - Reusability across multiple applications
    /// - Enhanced security through parameterization
    /// 
    /// Security:
    /// - Always use FormattableString for parameter interpolation
    /// - Stored procedures themselves provide another layer of SQL injection prevention
    /// </summary>
    public class ExecuteStoredProcedureCommandHandler(
        ISqlCommandRepository<Example> sqlCommandRepository
        ) : IRequestHandler<ExecuteStoredProcedureCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(ExecuteStoredProcedureCommand request, CancellationToken cancellationToken)
        {
            var result = await sqlCommandRepository.ExecuteStoredProcedureAsync(request.Sql, cancellationToken);
            return Result<int>.Success(result);
        }
    }
}
