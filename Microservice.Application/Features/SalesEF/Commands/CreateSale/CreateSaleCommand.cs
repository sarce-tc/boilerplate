using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.SalesEF.Commands.CreateSale;
// PATRÓN — Crea una venta PENDIENTE con sus ítems. El precio/nombre/IVA se toman del catálogo
// en el handler (no se confía en el cliente). Devuelve el PublicId (Result<Guid>).
public record CreateSaleCommand(
    Guid CashSessionPublicId,
    IReadOnlyList<CreateSaleItemRequest> Items,
    Guid? CustomerPublicId = null
) : IRequest<Result<Guid>>;
