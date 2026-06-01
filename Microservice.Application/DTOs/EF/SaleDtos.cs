using Microservice.Domain.Entities;

namespace Microservice.Application.DTOs.EF;

// DTOs de lectura/escritura de Ventas (POS).

/// <summary>Línea de venta.</summary>
public record SaleItemDto(
    Guid PublicId,
    Guid ProductPublicId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal LineNet,
    decimal LineTax,
    decimal LineTotal);

/// <summary>Venta con sus líneas y totales.</summary>
public record SaleDto(
    Guid PublicId,
    Guid? CustomerPublicId,
    Guid CashSessionPublicId,
    SaleStatus Status,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total,
    DateTimeOffset? ConfirmedAt,
    Guid? InvoicePublicId,
    IReadOnlyList<SaleItemDto> Items);

/// <summary>Vista liviana para listados paginados (sin líneas).</summary>
public record SalesPaginatedDto(
    Guid PublicId,
    Guid? CustomerPublicId,
    SaleStatus Status,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ConfirmedAt);

/// <summary>Ítem del body de POST /sales (el precio/nombre/IVA se toman del catálogo en el server).</summary>
public record CreateSaleItemRequest(Guid ProductPublicId, decimal Quantity);
