using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.CashEF.Commands.OpenCashSession;
// PATRÓN — Abre un turno de caja. Devuelve el PublicId (Result<Guid>).
public record OpenCashSessionCommand(
    string RegisterName,
    decimal OpeningBalance,
    string? OpenedBy = null
) : IRequest<Result<Guid>>;
