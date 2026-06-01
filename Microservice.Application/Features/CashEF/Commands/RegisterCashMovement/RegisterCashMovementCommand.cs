using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CashEF.Commands.RegisterCashMovement;
// PATRÓN — Registra un movimiento de efectivo en un turno abierto. Devuelve el PublicId del movimiento.
public record RegisterCashMovementCommand(
    Guid CashSessionPublicId,
    CashMovementType MovementType,
    decimal Amount,
    string? Description = null
) : IRequest<Result<Guid>>;
