using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.SalesEF.Commands.CancelSale;
// PATRÓN — Anula una venta PENDIENTE (las confirmadas requieren flujo de devolución — futuro).
public record CancelSaleCommand(Guid PublicId) : IRequest<Result<Guid>>;
