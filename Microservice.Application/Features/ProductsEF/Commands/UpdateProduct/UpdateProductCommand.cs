using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Features.ProductsEF.Commands.CreateProduct;

namespace Microservice.Application.Features.ProductsEF.Commands.UpdateProduct;
// PATRÓN — Actualiza un Product completo (scalar + gestión de códigos de barras) con change tracking.
// Campos opcionales: null = sin cambio (semántica PUT parcial, espejo de UpdateExampleCommand).
public record UpdateProductCommand(
    Guid PublicId,
    string? Name,
    string? Description,
    string? CategoryName,
    decimal? Price,
    decimal? Cost,
    decimal? TaxRate,
    IReadOnlyList<CreateProductBarcodeRequest>? AddBarcodes = null,
    IReadOnlyList<Guid>? RemoveBarcodeIds = null
) : IRequest<Result<Guid>>;
