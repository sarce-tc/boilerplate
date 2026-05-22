using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.Examples.Queries.GetExampleById
{
    public record GetExampleByIdQuery(
        int Id
    ) : IRequest<Result<GetExampleByIdDto>>;
}
