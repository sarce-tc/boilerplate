using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.GetExamplesWithProjection
{
    // EF projection (collection): IQueryRepository<T>.GetListAsync(selector, ct) — SELECT only specified columns
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
