using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Infrastructure;

namespace Microservice.Application.Features.SalesEF.Queries.GetSaleTicket;
// PATRÓN — Proyección imprimible de una venta (+ comprobante si existe). No persiste estado.
public record GetSaleTicketQuery(Guid SalePublicId) : IRequest<Result<TicketDocument>>;
