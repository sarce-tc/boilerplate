namespace Microservice.Client.Features.Sales.Models;

/// <summary>
/// A line in the in-progress sale cart. Mutable quantity (the only thing the cashier edits);
/// price/tax are snapshots taken from the catalog at scan time. Line totals are computed the
/// same way the backend does, but are INDICATIVE — the server recomputes authoritatively on create.
/// </summary>
public sealed class CartLineVm(Guid productPublicId, string sku, string name, decimal unitPrice, decimal taxRate)
{
    public Guid ProductPublicId { get; } = productPublicId;
    public string Sku { get; } = sku;
    public string Name { get; } = name;
    public decimal UnitPrice { get; } = unitPrice;
    public decimal TaxRate { get; } = taxRate;
    public decimal Quantity { get; set; } = 1;

    public decimal LineNet => Math.Round(UnitPrice * Quantity, 2);
    public decimal LineTax => Math.Round(LineNet * TaxRate / 100m, 2);
    public decimal LineTotal => LineNet + LineTax;
}

/// <summary>Row for the sales history grid.</summary>
public sealed record SaleListItemVm(
    Guid PublicId,
    SaleStatus Status,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ConfirmedAt,
    bool HasCustomer);

/// <summary>Confirmed-sale summary returned to the POS after checkout.</summary>
public sealed record SaleResultVm(
    Guid PublicId,
    SaleStatus Status,
    decimal Total,
    Guid? InvoicePublicId);

/// <summary>Renderable ticket (HTML content + content type) for the print dialog.</summary>
public sealed record TicketVm(string ContentType, string Content);
