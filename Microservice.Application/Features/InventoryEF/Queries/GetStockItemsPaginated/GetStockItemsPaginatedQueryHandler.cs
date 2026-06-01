using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Models;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.InventoryEF.Queries.GetStockItemsPaginated;
// PATRÓN — Paginación offset (espejo de GetExamplesPaginatedQueryHandler).
public sealed class GetStockItemsPaginatedQueryHandler(
    IReadRepository<StockItem> readRepository,
    IMapper mapper
) : IRequestHandler<GetStockItemsPaginatedQuery, Result<PagedResult<StockItemDto>>>
{
    public async Task<Result<PagedResult<StockItemDto>>> Handle(GetStockItemsPaginatedQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await readRepository.GetListPaginatedAsync(
            request.CurrentPage,
            request.PageSize,
            orderBy: q => q.OrderByDescending(s => s.UpdatedAt),
            cancellationToken: cancellationToken);

        var mappedResults = mapper.Map<IEnumerable<StockItemDto>>(pagedResult.Results);

        var result = new PagedResult<StockItemDto>(
            mappedResults,
            pagedResult.RowsCount,
            pagedResult.CurrentPage,
            pagedResult.PageSize);

        return Result<PagedResult<StockItemDto>>.Success(result);
    }
}
