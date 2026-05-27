using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleWithItems
{
    public record GetExampleWithItemsQuery(Guid PublicId) : IRequest<Result<GetExampleWithItemsDto>>;
}
