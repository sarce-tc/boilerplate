using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.GetExampleWithProjection
{
    // EF projection (single): IQueryRepository<T>.GetEntityAsync(selector, predicate, ct) — SELECT only specified columns
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
