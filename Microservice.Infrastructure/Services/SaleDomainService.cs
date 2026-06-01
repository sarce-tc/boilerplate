using Microservice.Application.Contracts.Interfaces;
using Microservice.Domain.Entities;
using Microservice.Domain.Exceptions;

namespace Microservice.Infrastructure.Services;

/// <summary>
/// Implementación de <see cref="ISaleDomainService"/>. Lógica de dominio pura — sin I/O.
/// Compone <see cref="IInventoryDomainService"/> para el ajuste de stock + asiento de inventario.
/// </summary>
public sealed class SaleDomainService(IInventoryDomainService inventoryService) : ISaleDomainService
{
    /// <inheritdoc/>
    public IReadOnlyList<InventoryMovement> Confirm(
        Sale sale,
        CashSession cashSession,
        IReadOnlyDictionary<Guid, StockItem> stockByProduct)
    {
        var reference = sale.PublicId.ToString();
        var movements = new List<InventoryMovement>(sale.Items.Count);

        // ── 1. Descontar stock de cada ítem (valida existencia y suficiencia) ──────
        foreach (var item in sale.Items)
        {
            if (!stockByProduct.TryGetValue(item.ProductPublicId, out var stock))
                throw new DomainException(
                    $"No stock record for product '{item.ProductPublicId}'; cannot confirm sale.");

            var movement = inventoryService.RegisterMovement(
                stock,
                InventoryMovementType.Sale,
                item.Quantity,
                reason: $"Sale {reference}",
                reference: reference);

            movements.Add(movement);
        }

        // ── 2. Registrar el cobro en la caja ───────────────────────────────────────
        cashSession.RegisterMovement(CashMovementType.Sale, sale.Total, $"Sale {reference}");

        // ── 3. Transicionar la venta a confirmada ───────────────────────────────────
        sale.Confirm();

        return movements;
    }
}
