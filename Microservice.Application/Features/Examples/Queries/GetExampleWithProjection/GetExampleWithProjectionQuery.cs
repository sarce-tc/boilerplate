using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.Examples.Queries.GetExampleWithProjection
{
    public record GetExampleWithProjectionQuery(
        Guid PublicId
    ) : IRequest<Result<GetExampleWithProjectionDto>>;
}
