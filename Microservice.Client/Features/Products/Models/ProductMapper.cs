namespace Microservice.Client.Features.Products.Models;

/// <summary>
/// Explicit DTO↔ViewModel mapping for the Products feature. No reflection/AutoMapper — the
/// archetype favours explicit, testable, trim-safe mappers. Every other feature mirrors this.
/// </summary>
public static class ProductMapper
{
    // ── DTO → VM (reads) ─────────────────────────────────────────────────────

    public static ProductListItemVm ToListItem(GetProductsPaginatedDto dto) =>
        new(dto.PublicId, dto.Sku, dto.Name, dto.Price, dto.TaxRate, dto.CategoryName, dto.IsActive);

    public static ProductFormModel ToFormModel(GetProductDto dto) => new()
    {
        PublicId = dto.PublicId,
        Sku = dto.Sku,
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        Cost = dto.Cost,
        TaxRate = dto.TaxRate,
        CategoryName = dto.CategoryName,
        IsActive = dto.IsActive,
        Barcodes = dto.Barcodes
            .Select(b => new BarcodeEntryVm { PublicId = b.PublicId, Code = b.Code, Symbology = b.Symbology })
            .ToList()
    };

    // ── VM → request DTO (writes) ────────────────────────────────────────────

    public static CreateProductRequest ToCreateRequest(ProductFormModel model) => new(
        Sku: model.Sku.Trim(),
        Name: model.Name.Trim(),
        Description: model.Description,
        Price: model.Price,
        Cost: model.Cost,
        TaxRate: model.TaxRate,
        CategoryName: model.CategoryName,
        Barcodes: model.Barcodes.Count == 0
            ? null
            : model.Barcodes.Select(b => new CreateProductBarcodeRequest(b.Code.Trim(), b.Symbology)).ToList());

    /// <summary>
    /// Build the update body. New barcodes (PublicId == null) go to AddBarcodes; the caller
    /// supplies the ids to remove. Scalar fields are always sent (the form is a full edit view).
    /// </summary>
    public static UpdateProductRequest ToUpdateRequest(ProductFormModel model, IReadOnlyList<Guid>? removeBarcodeIds = null)
    {
        var added = model.Barcodes
            .Where(b => b.PublicId is null)
            .Select(b => new CreateProductBarcodeRequest(b.Code.Trim(), b.Symbology))
            .ToList();

        return new UpdateProductRequest(
            Name: model.Name.Trim(),
            Description: model.Description,
            CategoryName: model.CategoryName,
            Price: model.Price,
            Cost: model.Cost,
            TaxRate: model.TaxRate,
            AddBarcodes: added.Count == 0 ? null : added,
            RemoveBarcodeIds: removeBarcodeIds is { Count: > 0 } ? removeBarcodeIds : null);
    }
}
