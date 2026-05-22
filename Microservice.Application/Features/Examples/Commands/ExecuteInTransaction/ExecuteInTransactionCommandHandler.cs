using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Commands.ExecuteInTransaction
{
    /// <summary>
    /// Use Case: Execute database operations within a transaction for data consistency.
    /// 
    /// When to use:
    /// - When multiple related operations must succeed or fail together (ACID compliance)
    /// - For critical business logic requiring atomicity guarantees
    /// - When data integrity is paramount (financial transactions, order processing)
    /// - For complex workflows spanning multiple database operations
    /// 
    /// Responsibilities:
    /// - Begin a database transaction
    /// - Execute the specified operation within the transaction context
    /// - Commit if successful, rollback on exception
    /// - Ensure data consistency across multiple changes
    /// 
    /// AI Agent Use Cases:
    /// - AI systems can use this to:
    ///   * Execute multi-step workflows atomically
    ///   * Create related entities with guaranteed consistency
    ///   * Perform complex data transformations safely
    ///   * Ensure AI-generated data maintains referential integrity
    /// 
    /// Safety Features:
    /// - Automatic rollback on any exception
    /// - Cancellation token support for timeout handling
    /// - Ensures database consistency even on partial failures
    /// 
    /// Performance Consideration:
    /// - Transactions hold locks; keep operations brief
    /// - Larger transactions may impact concurrency
    /// </summary>
    public class ExecuteInTransactionCommandHandler(
        ISqlRepository<Example> sqlRepository
        ) : IRequestHandler<ExecuteInTransactionCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(ExecuteInTransactionCommand request, CancellationToken cancellationToken)
        {
            var result = await sqlRepository.ExecuteInTransactionAsync(
                async (repository) =>
                {
                    var execResult = await repository.ExecuteSqlAsync(
                        $"INSERT INTO Examples (Description) VALUES ({request.Description})", 
                        cancellationToken);
                    return execResult;
                },
                cancellationToken);

            return Result<int>.Success(result);
        }
    }
}
