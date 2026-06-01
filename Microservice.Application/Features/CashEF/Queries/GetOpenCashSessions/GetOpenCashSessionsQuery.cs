using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.CashEF.Queries.GetOpenCashSessions;
// PATRÓN — Lista de turnos de caja actualmente abiertos.
public record GetOpenCashSessionsQuery : IRequest<Result<IReadOnlyList<CashSessionsPaginatedDto>>>;
