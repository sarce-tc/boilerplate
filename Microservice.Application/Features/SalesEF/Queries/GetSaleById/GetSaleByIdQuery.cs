using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.SalesEF.Queries.GetSaleById;
// PATRÓN — Detalle de una venta con sus líneas y totales.
public record GetSaleByIdQuery(Guid PublicId) : IRequest<Result<SaleDto>>;
