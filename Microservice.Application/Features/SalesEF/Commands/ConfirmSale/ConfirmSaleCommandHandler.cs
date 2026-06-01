using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Interfaces;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.SalesEF.Commands.ConfirmSale;
// PATRÓN — Domain service cross-aggregate (fila "Domain service" de COMMAND_PATTERNS):
//   1. precarga los aggregates tracked: Sale (con ítems), CashSession (con movimientos) y
//      los StockItem de cada producto vendido;
//   2. saleService.Confirm muta los tres (stock−, cobro+, venta→Confirmed) y devuelve los asientos;
//   3. un único SaveChangesAsync confirma todo en la TX implícita de EF.
// Las invariantes (caja abierta, stock suficiente, sin stock) lanzan DomainException → HTTP 409.
public sealed class ConfirmSaleCommandHandler(
    IReadRepository<Sale> saleRepository,
    IReadRepository<CashSession> cashRepository,
    IReadRepository<StockItem> stockRepository,
    ISaleDomainService saleService,
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<ConfirmSaleCommand, Result<SaleDto>>
{
    public async Task<Result<SaleDto>> Handle(ConfirmSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            includeProperties: [s => s.Items],
            disableTracking: false,
            cancellationToken: cancellationToken);

        if (sale is null)
            return Result<SaleDto>.Failure(Error.NotFound($"Sale {request.PublicId} not found."));

        var cashSession = await cashRepository.GetEntityAsync(
            x => x.PublicId == sale.CashSessionPublicId,
            includeProperties: [c => c.Movements],
            disableTracking: false,
            cancellationToken: cancellationToken);

        if (cashSession is null)
            return Result<SaleDto>.Failure(Error.NotFound($"Cash session {sale.CashSessionPublicId} not found."));

        var productIds = sale.Items.Select(i => i.ProductPublicId).Distinct().ToList();

        var stockItems = await stockRepository.GetListAsync(
            predicate: s => productIds.Contains(s.ProductPublicId),
            disableTracking: false,
            cancellationToken: cancellationToken);

        var stockByProduct = stockItems.ToDictionary(s => s.ProductPublicId);

        var movements = saleService.Confirm(sale, cashSession, stockByProduct);

        unitOfWork.SalesWrite.Update(sale);
        unitOfWork.CashSessionsWrite.Update(cashSession);
        foreach (var stock in stockByProduct.Values)
            unitOfWork.StockItemsWrite.Update(stock);
        foreach (var movement in movements)
            await unitOfWork.InventoryMovementsWrite.AddAsync(movement, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SaleDto>.Success(mapper.Map<SaleDto>(sale));
    }
}
