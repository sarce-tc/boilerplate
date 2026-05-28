using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleById;
public record GetExampleByIdQuery(
    int Id
) : IRequest<Result<GetExampleByIdDto>>;
