namespace Microservice.Client.Features.Sales.Models;

// ── Enum (mirror Microservice.Domain.SaleStatus) ─────────────────────────────
public enum SaleStatus { Pending = 0, Confirmed = 1, Cancelled = 2 }

// ── API contract DTOs (mirror Microservice.Application.DTOs.EF) ───────────────

public sealed record SaleItemDto(
    Guid PublicId,
    Guid ProductPublicId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal LineNet,
    decimal LineTax,
    decimal LineTotal);

public sealed record SaleDto(
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

public sealed record SalesPaginatedDto(
    Guid PublicId,
    Guid? CustomerPublicId,
    SaleStatus Status,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ConfirmedAt);

/// <summary>One line of POST /sales. Server takes price/name/tax from the catalog — client sends only product + qty.</summary>
public sealed record CreateSaleItemRequest(Guid ProductPublicId, decimal Quantity);

/// <summary>Body for POST /sales. Mirrors CreateSaleCommand.</summary>
public sealed record CreateSaleRequest(
    Guid CashSessionPublicId,
    IReadOnlyList<CreateSaleItemRequest> Items,
    Guid? CustomerPublicId = null);

/// <summary>Mirrors backend TicketDocument (GET /sales/{id}/ticket).</summary>
public sealed record TicketDocument(string ContentType, string Content);
