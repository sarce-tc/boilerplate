using Microservice.Domain.Entities;

namespace Microservice.Application.DTOs.EF;

// DTOs de lectura de Stock e Inventario.

/// <summary>Saldo materializado de un producto.</summary>
public record StockItemDto(
    Guid PublicId,
    Guid ProductPublicId,
    decimal QuantityOnHand);

/// <summary>Asiento del ledger de inventario.</summary>
public record InventoryMovementDto(
    Guid PublicId,
    Guid ProductPublicId,
    InventoryMovementType MovementType,
    decimal Quantity,
    decimal BalanceAfter,
    string? Reason,
    string? Reference,
    DateTimeOffset CreatedAt);
