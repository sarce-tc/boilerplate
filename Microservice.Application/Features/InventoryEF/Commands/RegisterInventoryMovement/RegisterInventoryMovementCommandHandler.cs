using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Interfaces;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.InventoryEF.Commands.RegisterInventoryMovement;
// PATRÓN — Domain service cross-aggregate (espejo de la fila "Domain service" de COMMAND_PATTERNS):
//   1. carga el StockItem tracked (o lo crea si es el primer movimiento del producto);
//   2. inventoryService.RegisterMovement muta el saldo (valida stock) y produce el asiento;
//   3. un único SaveChangesAsync confirma saldo + ledger en la TX implícita de EF.
// Las invariantes (cantidad > 0, stock suficiente) lanzan DomainException → HTTP 409.
public sealed class RegisterInventoryMovementCommandHandler(
    IReadRepository<StockItem> readRepository,
    IInventoryDomainService inventoryService,
    IUnitOfWork unitOfWork
) : IRequestHandler<RegisterInventoryMovementCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterInventoryMovementCommand request, CancellationToken cancellationToken)
    {
        var stock = await readRepository.GetEntityAsync(
            s => s.ProductPublicId == request.ProductPublicId,
            disableTracking: false,
            cancellationToken: cancellationToken);

        var isNew = stock is null;
        stock ??= new StockItem(request.ProductPublicId);

        var movement = inventoryService.RegisterMovement(
            stock,
            request.MovementType,
            request.Quantity,
            request.Reason,
            request.Reference);

        if (isNew)
            await unitOfWork.StockItemsWrite.AddAsync(stock, cancellationToken);
        else
            unitOfWork.StockItemsWrite.Update(stock);

        await unitOfWork.InventoryMovementsWrite.AddAsync(movement, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(movement.PublicId);
    }
}
