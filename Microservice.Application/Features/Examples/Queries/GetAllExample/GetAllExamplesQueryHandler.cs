using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.GetAllExample
{
    /// <summary>
    /// Use Case: Retrieve all Example records from the database.
    /// 
    /// When to use:
    /// - When a GET request is made to fetch all resources without filters
    /// - When AI agents need to analyze the complete dataset
    /// - When exporting or synchronizing data across systems
    /// - For dashboard operations that require an overview of all records
    /// 
    /// Responsibilities:
    /// - Query all entities from the read repository
    /// - Project entities to DTOs for API response
    /// - Handle mapping and data transformation
    /// 
    /// Performance Considerations:
    /// - For large datasets, consider using GetExamplesPaginatedQueryHandler instead
    /// - Implement caching via ICacheableQuery interface if applicable
    /// - AI agents analyzing patterns may benefit from cached results
    /// </summary>
    public class GetAllExamplesQueryHandler(
        IReadRepository<Example> readRepository,
        IMapper mapper
        ) : IRequestHandler<GetAllExamplesQuery, Result<IEnumerable<GetAllExamplesDto>>>
    {
        public async Task<Result<IEnumerable<GetAllExamplesDto>>> Handle(GetAllExamplesQuery request, CancellationToken cancellationToken)
        {
            var data = mapper.Map<IEnumerable<GetAllExamplesDto>>(await readRepository.GetListAsync(x => x.Id > 0, cancellationToken: cancellationToken));
            return Result<IEnumerable<GetAllExamplesDto>>.Success(data);
        }
    }
}
