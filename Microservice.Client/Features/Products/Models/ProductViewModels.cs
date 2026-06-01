namespace Microservice.Client.Features.Products.Models;

// ── UI view models ───────────────────────────────────────────────────────────
// Shaped for the UI, decoupled from the wire contract. Components bind these.

/// <summary>Row model for the catalog grid.</summary>
public sealed record ProductListItemVm(
    Guid PublicId,
    string Sku,
    string Name,
    decimal Price,
    decimal TaxRate,
    string? CategoryName,
    bool IsActive)
{
    /// <summary>Price with tax — a UI-only projection the DTO does not carry.</summary>
    public decimal PriceWithTax => Math.Round(Price * (1 + TaxRate / 100m), 2);
}

/// <summary>Barcode line within the edit form (mutable for binding).</summary>
public sealed class BarcodeEntryVm
{
    public Guid? PublicId { get; set; }   // null = new, not yet persisted
    public string Code { get; set; } = string.Empty;
    public string? Symbology { get; set; }
}

/// <summary>
/// Mutable edit/create model bound by ProductForm. Carries everything the form touches;
/// the mapper translates to the right request DTO depending on create vs update.
/// </summary>
public sealed class ProductFormModel
{
    public Guid? PublicId { get; set; }   // null = create
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public decimal TaxRate { get; set; } = 21m; // AR default IVA
    public string? CategoryName { get; set; }
    public bool IsActive { get; set; } = true;
    public List<BarcodeEntryVm> Barcodes { get; set; } = [];

    public bool IsNew => PublicId is null;
}
