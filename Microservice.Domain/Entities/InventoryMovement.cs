using Microservice.Domain.Exceptions;
using Microservice.Domain.ValueObjects;

namespace Microservice.Domain.Entities;

/// <summary>
/// Aggregate root del ledger de inventario (append-only): registra cada movimiento que altera
/// el saldo de un producto, con la cantidad, el tipo y el saldo resultante.
/// <para>
/// Lo crea el domain service de inventario al aplicar un movimiento sobre el <see cref="StockItem"/>.
/// Referencia al producto por <see cref="ProductPublicId"/> (sin navegación EF).
/// </para>
/// </summary>
public sealed class InventoryMovement : BaseDomainModel
{
    // ── Constraints ──────────────────────────────────────────────────────────
    public const int ReasonMaxLength = 300;
    public const int ReferenceMaxLength = 100;

    /// <summary>PublicId del <see cref="Product"/> afectado.</summary>
    public Guid ProductPublicId { get; private set; }

    /// <summary>Tipo de movimiento (define si suma o resta).</summary>
    public InventoryMovementType MovementType { get; private set; }

    /// <summary>Magnitud del movimiento (siempre positiva).</summary>
    public decimal Quantity { get; private set; }

    /// <summary>Saldo del producto inmediatamente después de aplicar este movimiento.</summary>
    public decimal BalanceAfter { get; private set; }

    /// <summary>Motivo libre del movimiento.</summary>
    public string? Reason { get; private set; }

    /// <summary>Referencia externa (p.ej. PublicId de la venta o de la compra).</summary>
    public string? Reference { get; private set; }

    // ── EF Core parameterless constructor (infrastructure only) ──────────────
    private InventoryMovement() { }

    /// <summary>Factory: lo invoca el domain service de inventario tras ajustar el saldo.</summary>
    /// <exception cref="DomainException">Cantidad no positiva.</exception>
    public InventoryMovement(
        Guid productPublicId,
        InventoryMovementType movementType,
        decimal quantity,
        decimal balanceAfter,
        string? reason,
        string? reference)
    {
        if (productPublicId == Guid.Empty)
            throw new ArgumentException("ProductPublicId is required.", nameof(productPublicId));
        if (quantity <= 0)
            throw new DomainException("Movement quantity must be greater than zero.");

        ProductPublicId = productPublicId;
        MovementType    = movementType;
        Quantity        = quantity;
        BalanceAfter    = balanceAfter;
        Reason          = reason?.Trim();
        Reference       = reference?.Trim();
        PublicId        = Guid.NewGuid();
        CreatedAt       = DateTimeOffset.UtcNow;
        UpdatedAt       = DateTimeOffset.UtcNow;
    }
}
