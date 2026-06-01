using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.ProductsEF.Queries.GetProductById;
// PATRÓN — Detalle de un Product (con sus códigos de barras) por PublicId.
public record GetProductByIdQuery(Guid PublicId) : IRequest<Result<GetProductDto>>;
