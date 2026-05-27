using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleItemByPublicId
{
    public record GetExampleItemByPublicIdQuery(
        Guid ExamplePublicId,
        Guid ItemPublicId
    ) : IRequest<Result<GetExampleItemDto>>;
}
