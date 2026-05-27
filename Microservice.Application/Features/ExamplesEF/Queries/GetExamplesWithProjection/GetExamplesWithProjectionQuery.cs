using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExamplesWithProjection
{
    public record GetExamplesWithProjectionQuery : IRequest<Result<IEnumerable<GetExamplesWithProjectionDto>>>;
}
