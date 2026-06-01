namespace Microservice.Client.Features.Inventory.Models;

/// <summary>Mirrors Domain InventoryMovementType. Inbound (+) vs outbound (−) per the comments below.</summary>
public enum InventoryMovementType
{
    Purchase = 0,       // +
    Sale = 1,           // −
    Return = 2,         // +
    AdjustmentIn = 3,   // +
    AdjustmentOut = 4,  // −
    Loss = 5,           // −
    InitialLoad = 6     // +
}

// ── API contract DTOs (mirror Microservice.Application.DTOs.EF) ───────────────

/// <summary>Materialized on-hand balance for a product.</summary>
public sealed record StockItemDto(Guid PublicId, Guid ProductPublicId, decimal QuantityOnHand);

/// <summary>A ledger entry. BalanceAfter is server-authoritative.</summary>
public sealed record InventoryMovementDto(
    Guid PublicId,
    Guid ProductPublicId,
    InventoryMovementType MovementType,
    decimal Quantity,
    decimal BalanceAfter,
    string? Reason,
    string? Reference,
    DateTimeOffset CreatedAt);

/// <summary>Body for POST /inventory/movements. Mirrors RegisterInventoryMovementCommand.</summary>
public sealed record RegisterInventoryMovementRequest(
    Guid ProductPublicId,
    InventoryMovementType MovementType,
    decimal Quantity,
    string? Reason = null,
    string? Reference = null);
