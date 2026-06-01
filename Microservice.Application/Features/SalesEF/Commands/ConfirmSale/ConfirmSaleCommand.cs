using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.SalesEF.Commands.ConfirmSale;
// PATRÓN — Confirma la venta: descuenta stock, registra el cobro en caja y la marca confirmada.
// Devuelve la venta confirmada con sus totales.
public record ConfirmSaleCommand(Guid PublicId) : IRequest<Result<SaleDto>>;
