using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.InventoryEF.Commands.RegisterInventoryMovement;
// PATRÓN — Registra un movimiento de inventario que ajusta el saldo del producto.
// Devuelve el PublicId del asiento creado (Result<Guid>).
public record RegisterInventoryMovementCommand(
    Guid ProductPublicId,
    InventoryMovementType MovementType,
    decimal Quantity,
    string? Reason = null,
    string? Reference = null
) : IRequest<Result<Guid>>;
