using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.CashEF.Queries.GetCashSessionById;
// PATRÓN — Detalle de un turno de caja con sus movimientos.
public record GetCashSessionByIdQuery(Guid PublicId) : IRequest<Result<CashSessionDto>>;
