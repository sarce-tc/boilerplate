using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.InventoryEF.Queries.GetStockByProduct;
// PATRÓN — Saldo actual de un producto por su PublicId.
public record GetStockByProductQuery(Guid ProductPublicId) : IRequest<Result<StockItemDto>>;
