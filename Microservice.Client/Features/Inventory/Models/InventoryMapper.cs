namespace Microservice.Client.Features.Inventory.Models;

/// <summary>Explicit DTO↔VM mapping for Inventory (archetype-consistent).</summary>
public static class InventoryMapper
{
    public static StockItemVm ToStockItem(StockItemDto dto) =>
        new(dto.ProductPublicId, dto.QuantityOnHand);

    public static InventoryMovementVm ToMovement(InventoryMovementDto dto) =>
        new(dto.PublicId, dto.MovementType, dto.Quantity, dto.BalanceAfter, dto.Reason, dto.Reference, dto.CreatedAt);
}
