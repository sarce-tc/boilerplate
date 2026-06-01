using Microservice.Application.Contracts.Interfaces;
using Microservice.Domain.Entities;

namespace Microservice.Infrastructure.Services;

/// <summary>
/// Implementación de <see cref="IInventoryDomainService"/>.
/// Lógica de dominio pura — sin I/O ni dependencias de infraestructura.
/// </summary>
public sealed class InventoryDomainService : IInventoryDomainService
{
    /// <inheritdoc/>
    public InventoryMovement RegisterMovement(
        StockItem stock,
        InventoryMovementType movementType,
        decimal quantity,
        string? reason,
        string? reference)
    {
        // ── 1. Ajustar el saldo — el StockItem valida cantidad y stock suficiente ──
        if (IInventoryDomainService.IsIncrease(movementType))
            stock.Increase(quantity);
        else
            stock.Decrease(quantity);

        // ── 2. Asiento del ledger con el saldo ya actualizado ──────────────────────
        return new InventoryMovement(
            stock.ProductPublicId,
            movementType,
            quantity,
            stock.QuantityOnHand,
            reason,
            reference);
    }
}
