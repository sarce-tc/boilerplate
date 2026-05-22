using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.ExistsExample
{
    /// <summary>
    /// Use Case: Check if an Example record exists in the database.
    /// 
    /// When to use:
    /// - Before attempting update or delete operations (guard clause)
    /// - For validation endpoints that check resource existence
    /// - When validating foreign key references exist
    /// - For efficient existence checks without retrieving full data
    /// 
    /// Responsibilities:
    /// - Check if entity exists matching the given predicate
    /// - Return boolean result without loading full entity data
    /// - Provide efficient query execution
    /// 
    /// AI Agent Use Cases:
    /// - AI systems can use this to:
    ///   * Validate referenced entities before creation
    ///   * Check for duplicates before generating content
    ///   * Verify prerequisites for data transformations
    ///   * Implement smart conditional workflows
    /// 
    /// Performance Advantages:
    /// - Minimal database query (EXISTS check in SQL)
    /// - Doesn't load full entity into memory
    /// - Ideal for validation and guard clauses
    /// - Reduced network overhead vs full entity retrieval
    /// 
    /// Use in Validation:
    /// - Before creating entities with foreign key dependencies
    /// - To prevent creating duplicate records
    /// - To validate workflow prerequisites
    /// </summary>
    public class ExistsExampleQueryHandler(
        IReadRepository<Example> readRepository
        ) : IRequestHandler<ExistsExampleQuery, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ExistsExampleQuery request, CancellationToken cancellationToken)
        {
            var exists = await readRepository.ExistsAsync(x => x.PublicId == request.PublicId, cancellationToken);
            return Result<bool>.Success(exists);
        }
    }
}
