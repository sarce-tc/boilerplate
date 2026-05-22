using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.GetExampleById
{
    /// <summary>
    /// Use Case: Retrieve a specific Example record by its unique identifier.
    /// 
    /// When to use:
    /// - When receiving a GET request with a specific ID parameter
    /// - When AI agents need to fetch details about a particular entity
    /// - When validating entity existence before performing operations
    /// - For displaying detailed views in UI or external systems
    /// 
    /// Responsibilities:
    /// - Query the database for a single entity by ID
    /// - Project the entity to a DTO for response
    /// - Handle mapping and transformation
    /// - Return not-found gracefully for missing records
    /// 
    /// AI Agent Integration:
    /// - Useful for AI agents to:
    ///   * Fetch entity details for analysis or processing
    ///   * Validate references before creating relationships
    ///   * Retrieve context for decision-making operations
    /// 
    /// Performance:
    /// - Optimized for single record retrieval
    /// - Leverages primary key lookup for fast access
    /// </summary>
    public class GetExampleByIdQueryHandler(
        IReadRepository<Example> readRepository,
        IMapper mapper
        ) : IRequestHandler<GetExampleByIdQuery, Result<GetExampleByIdDto>>
    {
        public async Task<Result<GetExampleByIdDto>> Handle(GetExampleByIdQuery request, CancellationToken cancellationToken)
        {
            var example = await readRepository.FindAsync(request.Id, cancellationToken);

            if (example is null)
                return Result<GetExampleByIdDto>.Failure(Error.NotFound("Ejemplo no encontrado"));

            return Result<GetExampleByIdDto>.Success(mapper.Map<GetExampleByIdDto>(example));
        }
    }
}
