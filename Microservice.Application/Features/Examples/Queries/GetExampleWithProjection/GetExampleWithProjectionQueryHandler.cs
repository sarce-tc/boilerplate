using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.GetExampleWithProjection
{
    /// <summary>
    /// Use Case: Retrieve a single entity with field projection (select specific columns only).
    /// 
    /// When to use:
    /// - Fetching a specific record with only needed fields
    /// - Detail views where some fields are optional
    /// - Optimizing response payload size
    /// - API endpoints returning lightweight entity representations
    /// 
    /// Responsibilities:
    /// - Use IQueryRepository for projection capability
    /// - Project entity to DTO selecting only required fields
    /// - Find entity by ID or predicate
    /// - Return projected data or not-found result
    /// 
    /// Performance Benefits:
    /// - SQL SELECT includes only specified columns
    /// - Reduced data transfer vs full entity retrieval
    /// - Lower memory usage by avoiding unnecessary fields
    /// - Optimized for bandwidth-constrained scenarios
    /// 
    /// Comparison:
    /// - GetExampleByIdQueryHandler: Returns full entity fields
    /// - GetExampleWithProjectionQueryHandler: Returns only projected fields
    /// 
    /// AI Agent Use Cases:
    /// - Retrieve specific fields for AI model input
    /// - Optimize API calls when only certain fields are needed
    /// - Create lightweight representations for agent-to-agent communication
    /// - Reduce memory footprint for high-volume queries
    /// 
    /// Example Use Case:
    /// - Fetch only Id and Status for workflow decision-making
    /// - Return only Id and CreatedDate for timeline views
    /// - Retrieve minimal fields for filtering/matching operations
    /// </summary>
    public class GetExampleWithProjectionQueryHandler(
        IQueryRepository<Example> queryRepository
        ) : IRequestHandler<GetExampleWithProjectionQuery, Result<GetExampleWithProjectionDto>>
    {
        public async Task<Result<GetExampleWithProjectionDto>> Handle(GetExampleWithProjectionQuery request, CancellationToken cancellationToken)
        {
            var result = await queryRepository.GetEntityAsync(x => new GetExampleWithProjectionDto(x.PublicId, x.Name, x.Description), x => x.PublicId == request.PublicId, cancellationToken);
            
            if (result is null)
                return Result<GetExampleWithProjectionDto>.Failure(Error.NotFound("Ejemplo no encontrado"));
            
            return Result<GetExampleWithProjectionDto>.Success(result);
        }
    }
}
