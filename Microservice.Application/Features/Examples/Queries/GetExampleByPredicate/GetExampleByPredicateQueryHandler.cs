using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;
using System.Linq.Expressions;

namespace Microservice.Application.Features.Examples.Queries.GetExampleByPredicate
{
    /// <summary>
    /// Use Case: Retrieve a single entity matching custom filter criteria.
    /// 
    /// When to use:
    /// - Finding entities by non-ID fields (email, code, name, etc.)
    /// - Querying with complex predicates
    /// - Flexible search scenarios defined at runtime
    /// - Validation lookups using business keys
    /// 
    /// Responsibilities:
    /// - Build custom predicate from request parameters
    /// - Query for matching entity
    /// - Return DTO projection of the entity
    /// 
    /// AI Agent Use Cases:
    /// - AI agents searching for entities by extracted attributes
    /// - Finding records matching AI-generated search criteria
    /// - Lookup operations based on ML analysis results
    /// - Validation of entities before processing
    /// 
    /// Difference from GetExampleByIdQueryHandler:
    /// - GetExampleById: Fixed lookup by primary key (fast index lookup)
    /// - GetExampleByPredicate: Flexible search by any criteria (may require table scan)
    /// 
    /// Performance Note:
    /// - Depending on predicate, may require full table scan if no index exists
    /// - Ensure indexed fields are used in predicate for optimal performance
    /// </summary>
    public class GetExampleByPredicateQueryHandler(
        IReadRepository<Example> readRepository,
        IMapper mapper
        ) : IRequestHandler<GetExampleByPredicateQuery, Result<GetExampleByPredicateDto>>
    {
        public async Task<Result<GetExampleByPredicateDto>> Handle(GetExampleByPredicateQuery request, CancellationToken cancellationToken)
        {
            var example = await readRepository.GetEntityAsync(x => x.PublicId == request.PublicId, cancellationToken: cancellationToken);

            if (example is null)
                return Result<GetExampleByPredicateDto>.Failure(Error.NotFound("Ejemplo no encontrado"));

            return Result<GetExampleByPredicateDto>.Success(mapper.Map<GetExampleByPredicateDto>(example));
        }
    }
}
