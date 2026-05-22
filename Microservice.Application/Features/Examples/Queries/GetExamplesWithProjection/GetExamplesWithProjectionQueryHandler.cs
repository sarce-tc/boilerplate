using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.GetExamplesWithProjection
{
    /// <summary>
    /// Use Case: Retrieve multiple entities with field projection (select specific columns only).
    /// 
    /// When to use:
    /// - Fetching only needed fields for list views (avoids unnecessary data)
    /// - Optimizing network bandwidth by limiting returned data
    /// - API endpoints where full entity data is not required
    /// - Creating lightweight DTOs for specific UI screens
    /// 
    /// Responsibilities:
    /// - Use IQueryRepository for projection capability
    /// - Select only the columns needed in the response
    /// - Return DTO collection with projected fields
    /// 
    /// Performance Benefits:
    /// - SQL SELECT statement includes only specified columns
    /// - Reduced database I/O (smaller result sets)
    /// - Lower network bandwidth consumption
    /// - Faster data transfer and deserialization
    /// - Better cache efficiency
    /// 
    /// Comparison with GetAllExamplesQueryHandler:
    /// - GetAllExamples: Returns all entity fields
    /// - GetExamplesWithProjection: Returns only specified fields via projection
    /// 
    /// AI Agent Use Cases:
    /// - Project only fields relevant for AI processing
    /// - Optimize API responses for high-volume AI queries
    /// - Create lightweight DTOs for batch AI analysis
    /// - Reduce payload size when querying from multiple AI agents
    /// 
    /// Example Projection:
    /// - Instead of: SELECT Id, Name, Description, Status, CreatedDate, ModifiedDate...
    /// - Use: SELECT Id, Name (only needed fields)
    /// </summary>
    public class GetExamplesWithProjectionQueryHandler(
        IQueryRepository<Example> queryRepository
        ) : IRequestHandler<GetExamplesWithProjectionQuery, Result<IEnumerable<GetExamplesWithProjectionDto>>>
    {
        public async Task<Result<IEnumerable<GetExamplesWithProjectionDto>>> Handle(GetExamplesWithProjectionQuery request, CancellationToken cancellationToken)
        {
            var data = await queryRepository.GetListAsync(x => new GetExamplesWithProjectionDto(x.PublicId, x.Name, x.Description), cancellationToken: cancellationToken);
            return Result<IEnumerable<GetExamplesWithProjectionDto>>.Success(data);
        }
    }
}
