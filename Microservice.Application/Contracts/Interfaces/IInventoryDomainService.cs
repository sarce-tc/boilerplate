using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Interfaces;

/// <summary>
/// Domain service para operaciones cross-aggregate de inventario.
/// <para>
/// Coordina <see cref="StockItem"/> (saldo materializado) y <see cref="InventoryMovement"/> (ledger):
/// ajusta el saldo según el tipo de movimiento y produce el asiento del ledger.
/// </para>
/// <para>
/// Implementación = lógica de dominio pura (sin I/O); la atomicidad de persistencia es
/// responsabilidad del caller vía <c>IUnitOfWork</c> (Update del StockItem + Add del movimiento + SaveChanges).
/// </para>
/// </summary>
public interface IInventoryDomainService
{
    /// <summary>
    /// Aplica un movimiento sobre <paramref name="stock"/> (suma o resta según
    /// <paramref name="movementType"/>) y devuelve el asiento de ledger resultante.
    /// </summary>
    /// <exception cref="Exceptions.DomainException">
    /// Cantidad no positiva o saldo insuficiente para un egreso.
    /// </exception>
    InventoryMovement RegisterMovement(
        StockItem stock,
        InventoryMovementType movementType,
        decimal quantity,
        string? reason,
        string? reference);

    /// <summary>Indica si el tipo de movimiento incrementa (true) o decrementa (false) el saldo.</summary>
    static bool IsIncrease(InventoryMovementType type) => type switch
    {
        InventoryMovementType.Purchase
        or InventoryMovementType.Return
        or InventoryMovementType.AdjustmentIn
        or InventoryMovementType.InitialLoad => true,
        _ => false,
    };
}
