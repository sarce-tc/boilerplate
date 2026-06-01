namespace Microservice.Client.Features.Products.Models;

// ── API contract DTOs ────────────────────────────────────────────────────────
// Exact mirrors of the backend wire types (Microservice.Application.DTOs.EF +
// CreateProductCommand / UpdateProductRequestDto). These are SERIALIZATION types only:
// never bound directly to inputs — components bind ViewModels and map explicitly.

/// <summary>Mirrors backend ProductBarcodeDto.</summary>
public sealed record ProductBarcodeDto(Guid PublicId, string Code, string? Symbology);

/// <summary>Mirrors backend GetProductDto (detail view).</summary>
public sealed record GetProductDto(
    Guid PublicId,
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    decimal Cost,
    decimal TaxRate,
    string? CategoryName,
    bool IsActive,
    IReadOnlyList<ProductBarcodeDto> Barcodes);

/// <summary>Mirrors backend GetProductsPaginatedDto (list row).</summary>
public sealed record GetProductsPaginatedDto(
    Guid PublicId,
    string Sku,
    string Name,
    decimal Price,
    decimal TaxRate,
    string? CategoryName,
    bool IsActive);

/// <summary>Mirrors backend CreateProductBarcodeRequest.</summary>
public sealed record CreateProductBarcodeRequest(string Code, string? Symbology);

/// <summary>Request body for POST /products. Mirrors backend CreateProductCommand.</summary>
public sealed record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    decimal Cost,
    decimal TaxRate,
    string? CategoryName,
    IReadOnlyList<CreateProductBarcodeRequest>? Barcodes = null);

/// <summary>Request body for PUT /products/{publicId}. Mirrors backend UpdateProductRequestDto (null = unchanged).</summary>
public sealed record UpdateProductRequest(
    string? Name,
    string? Description,
    string? CategoryName,
    decimal? Price,
    decimal? Cost,
    decimal? TaxRate,
    IReadOnlyList<CreateProductBarcodeRequest>? AddBarcodes,
    IReadOnlyList<Guid>? RemoveBarcodeIds);
