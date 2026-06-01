namespace Microservice.Client.Features.Inventory.Models;

/// <summary>Stock balance row, optionally enriched with the product name/SKU (resolved by the state).</summary>
public sealed record StockItemVm(
    Guid ProductPublicId,
    decimal QuantityOnHand,
    string? ProductName = null,
    string? Sku = null)
{
    public bool IsOutOfStock => QuantityOnHand <= 0;
}

/// <summary>A movement row for the ledger, with a UI-friendly signed quantity.</summary>
public sealed record InventoryMovementVm(
    Guid PublicId,
    InventoryMovementType MovementType,
    decimal Quantity,
    decimal BalanceAfter,
    string? Reason,
    string? Reference,
    DateTimeOffset CreatedAt)
{
    /// <summary>Signed by direction (inbound + / outbound −) for display.</summary>
    public decimal SignedQuantity => InventoryLabels.IsInbound(MovementType) ? Quantity : -Quantity;
}

/// <summary>Mutable model bound by the register-movement dialog.</summary>
public sealed class MovementFormModel
{
    /// <summary>Barcode/SKU typed to look up the product.</summary>
    public string Lookup { get; set; } = string.Empty;

    public Guid ProductPublicId { get; set; }
    public string? ProductDisplay { get; set; } // resolved name/SKU shown to the user
    public InventoryMovementType MovementType { get; set; } = InventoryMovementType.Purchase;
    public decimal Quantity { get; set; } = 1;
    public string? Reason { get; set; }
    public string? Reference { get; set; }

    public bool HasProduct => ProductPublicId != Guid.Empty;

    public RegisterInventoryMovementRequest ToRequest() => new(
        ProductPublicId, MovementType, Quantity,
        string.IsNullOrWhiteSpace(Reason) ? null : Reason.Trim(),
        string.IsNullOrWhiteSpace(Reference) ? null : Reference.Trim());
}

/// <summary>Single source for movement-type labels and direction.</summary>
public static class InventoryLabels
{
    public static bool IsInbound(InventoryMovementType t) => t is
        InventoryMovementType.Purchase or InventoryMovementType.Return or
        InventoryMovementType.AdjustmentIn or InventoryMovementType.InitialLoad;

    public static string Movement(InventoryMovementType t) => t switch
    {
        InventoryMovementType.Purchase => "Compra",
        InventoryMovementType.Sale => "Venta",
        InventoryMovementType.Return => "Devolución",
        InventoryMovementType.AdjustmentIn => "Ajuste (+)",
        InventoryMovementType.AdjustmentOut => "Ajuste (−)",
        InventoryMovementType.Loss => "Merma / pérdida",
        InventoryMovementType.InitialLoad => "Carga inicial",
        _ => t.ToString()
    };
}
