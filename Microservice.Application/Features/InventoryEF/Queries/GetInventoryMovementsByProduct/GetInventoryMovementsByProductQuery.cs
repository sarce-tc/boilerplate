using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.InventoryEF.Queries.GetInventoryMovementsByProduct;
// PATRÓN — Ledger de movimientos de un producto (más recientes primero).
public record GetInventoryMovementsByProductQuery(Guid ProductPublicId)
    : IRequest<Result<IReadOnlyList<InventoryMovementDto>>>;
