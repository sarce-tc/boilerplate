using Microservice.Application.Features.ProductsEF.Commands.CreateProduct;

namespace Microservice.Application.DTOs.EF;

// DTOs de lectura del aggregate Product (records planos, mapeo por convención AutoMapper).

/// <summary>Código de barras proyectado para respuestas de lectura.</summary>
public record ProductBarcodeDto(
    Guid PublicId,
    string Code,
    string? Symbology);

/// <summary>Producto con su colección de códigos de barras (vista de detalle).</summary>
public record GetProductDto(
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

/// <summary>Vista liviana para listados paginados (sin códigos de barras).</summary>
public record GetProductsPaginatedDto(
    Guid PublicId,
    string Sku,
    string Name,
    decimal Price,
    decimal TaxRate,
    string? CategoryName,
    bool IsActive);

/// <summary>Body de PUT /products/{publicId}. Campos null = sin cambio.</summary>
public record UpdateProductRequestDto(
    string? Name,
    string? Description,
    string? CategoryName,
    decimal? Price,
    decimal? Cost,
    decimal? TaxRate,
    IReadOnlyList<CreateProductBarcodeRequest>? AddBarcodes,
    IReadOnlyList<Guid>? RemoveBarcodeIds);
