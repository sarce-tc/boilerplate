using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.InventoryEF.Queries.GetStockByProduct;
// PATRÓN — Lectura por predicado (generic-first): GetEntityAsync(s => s.ProductPublicId == id).
public sealed class GetStockByProductQueryHandler(
    IReadRepository<StockItem> readRepository,
    IMapper mapper
) : IRequestHandler<GetStockByProductQuery, Result<StockItemDto>>
{
    public async Task<Result<StockItemDto>> Handle(GetStockByProductQuery request, CancellationToken cancellationToken)
    {
        var stock = await readRepository.GetEntityAsync(
            x => x.ProductPublicId == request.ProductPublicId,
            cancellationToken: cancellationToken);

        if (stock is null)
            return Result<StockItemDto>.Failure(Error.NotFound($"No stock record for product '{request.ProductPublicId}'."));

        return Result<StockItemDto>.Success(mapper.Map<StockItemDto>(stock));
    }
}
