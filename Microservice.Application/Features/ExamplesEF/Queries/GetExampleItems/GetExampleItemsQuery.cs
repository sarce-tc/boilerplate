using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleItems
{
    public record GetExampleItemsQuery(Guid ExamplePublicId) : IRequest<Result<IEnumerable<GetExampleItemDto>>>;
}
