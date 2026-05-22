using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;
using Microservice.Application.Models;

namespace Microservice.Application.Features.Examples.Queries.GetExamplesPaginated
{
    public record GetExamplesPaginatedQuery(
        int CurrentPage,
        int PageSize
    ) : IRequest<Result<PagedResult<GetExamplesPaginatedDto>>>;
}
