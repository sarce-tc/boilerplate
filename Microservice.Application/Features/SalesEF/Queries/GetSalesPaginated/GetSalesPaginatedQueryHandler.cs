using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Models;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.SalesEF.Queries.GetSalesPaginated;
// PATRÓN — Paginación offset (espejo de GetExamplesPaginatedQueryHandler).
public sealed class GetSalesPaginatedQueryHandler(
    IReadRepository<Sale> readRepository,
    IMapper mapper
) : IRequestHandler<GetSalesPaginatedQuery, Result<PagedResult<SalesPaginatedDto>>>
{
    public async Task<Result<PagedResult<SalesPaginatedDto>>> Handle(GetSalesPaginatedQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await readRepository.GetListPaginatedAsync(
            request.CurrentPage,
            request.PageSize,
            orderBy: q => q.OrderByDescending(s => s.CreatedAt),
            cancellationToken: cancellationToken);

        var mappedResults = mapper.Map<IEnumerable<SalesPaginatedDto>>(pagedResult.Results);

        var result = new PagedResult<SalesPaginatedDto>(
            mappedResults,
            pagedResult.RowsCount,
            pagedResult.CurrentPage,
            pagedResult.PageSize);

        return Result<PagedResult<SalesPaginatedDto>>.Success(result);
    }
}
