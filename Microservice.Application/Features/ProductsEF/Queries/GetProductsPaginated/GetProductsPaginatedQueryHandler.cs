using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Models;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ProductsEF.Queries.GetProductsPaginated;
// PATRÓN — Paginación offset (espejo de GetExamplesPaginatedQueryHandler): COUNT(*) + LIMIT/OFFSET.
public sealed class GetProductsPaginatedQueryHandler(
    IReadRepository<Product> readRepository,
    IMapper mapper
) : IRequestHandler<GetProductsPaginatedQuery, Result<PagedResult<GetProductsPaginatedDto>>>
{
    public async Task<Result<PagedResult<GetProductsPaginatedDto>>> Handle(GetProductsPaginatedQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await readRepository.GetListPaginatedAsync(
            request.CurrentPage,
            request.PageSize,
            orderBy: q => q.OrderBy(p => p.Name),
            cancellationToken: cancellationToken);

        var mappedResults = mapper.Map<IEnumerable<GetProductsPaginatedDto>>(pagedResult.Results);

        var result = new PagedResult<GetProductsPaginatedDto>(
            mappedResults,
            pagedResult.RowsCount,
            pagedResult.CurrentPage,
            pagedResult.PageSize);

        return Result<PagedResult<GetProductsPaginatedDto>>.Success(result);
    }
}
