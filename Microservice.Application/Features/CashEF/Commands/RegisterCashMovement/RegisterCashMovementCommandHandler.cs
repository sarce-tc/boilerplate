using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CashEF.Commands.RegisterCashMovement;
// PATRÓN — Actualizar aggregate con hijos (espejo de UpdateExampleCommandHandler):
// carga la sesión tracked + includeProperties Movements, aplica el domain method, SaveChanges.
// La regla "caja abierta" la valida el aggregate → DomainException → HTTP 409.
public sealed class RegisterCashMovementCommandHandler(
    IReadRepository<CashSession> readRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RegisterCashMovementCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterCashMovementCommand request, CancellationToken cancellationToken)
    {
        var session = await readRepository.GetEntityAsync(
            x => x.PublicId == request.CashSessionPublicId,
            includeProperties: [s => s.Movements],
            disableTracking: false,
            cancellationToken: cancellationToken);

        if (session is null)
            return Result<Guid>.Failure(Error.NotFound($"Cash session {request.CashSessionPublicId} not found."));

        var movement = session.RegisterMovement(request.MovementType, request.Amount, request.Description);

        unitOfWork.CashSessionsWrite.Update(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(movement.PublicId);
    }
}
