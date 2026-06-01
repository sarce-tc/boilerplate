using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ProductsEF.Commands.CreateProduct;
// PATRÓN — Crea un Product con colección opcional de códigos de barras en el mismo request.
// Devuelve el PublicId del producto creado (Result<Guid>).
public record CreateProductCommand(
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    decimal Cost,
    decimal TaxRate,
    string? CategoryName,
    IReadOnlyList<CreateProductBarcodeRequest>? Barcodes = null
) : IRequest<Result<Guid>>;

public record CreateProductBarcodeRequest(string Code, string? Symbology);
