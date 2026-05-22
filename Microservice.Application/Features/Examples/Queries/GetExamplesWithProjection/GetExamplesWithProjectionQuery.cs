using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.Examples.Queries.GetExamplesWithProjection
{
    public record GetExamplesWithProjectionQuery : IRequest<Result<IEnumerable<GetExamplesWithProjectionDto>>>;
}
