using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.ProductsEF.Queries.GetProductByBarcode;
// PATRÓN — Resolución de escaneo: dado el código leído por el lector, devuelve el Product.
public record GetProductByBarcodeQuery(string Code) : IRequest<Result<GetProductDto>>;
