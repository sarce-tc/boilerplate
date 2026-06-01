using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CashEF.Commands.CloseCashSession;
// PATRÓN — Actualizar aggregate con hijos: carga la sesión tracked + Movements, aplica Close
// (calcula esperado/diferencia internamente) y devuelve el resumen del arqueo.
public sealed class CloseCashSessionCommandHandler(
    IReadRepository<CashSession> readRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<CloseCashSessionCommand, Result<CashSessionDto>>
{
    public async Task<Result<CashSessionDto>> Handle(CloseCashSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await readRepository.GetEntityAsync(
            x => x.PublicId == request.CashSessionPublicId,
            includeProperties: [s => s.Movements],
            disableTracking: false,
            cancellationToken: cancellationToken);

        if (session is null)
            return Result<CashSessionDto>.Failure(Error.NotFound($"Cash session {request.CashSessionPublicId} not found."));

        session.Close(request.DeclaredBalance, request.ClosedBy);

        unitOfWork.CashSessionsWrite.Update(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CashSessionDto>.Success(mapper.Map<CashSessionDto>(session));
    }
}
