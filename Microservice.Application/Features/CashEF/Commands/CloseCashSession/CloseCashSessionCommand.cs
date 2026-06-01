using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.CashEF.Commands.CloseCashSession;
// PATRÓN — Cierra un turno de caja con arqueo. Devuelve el resumen con el esperado y la diferencia.
public record CloseCashSessionCommand(
    Guid CashSessionPublicId,
    decimal DeclaredBalance,
    string? ClosedBy = null
) : IRequest<Result<CashSessionDto>>;
