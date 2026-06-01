using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CashEF.Commands.OpenCashSession;
// PATRÓN — Crear aggregate raíz (espejo de CreateExampleCommandHandler).
public sealed class OpenCashSessionCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<OpenCashSessionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(OpenCashSessionCommand request, CancellationToken cancellationToken)
    {
        var session = mapper.Map<CashSession>(request);

        await unitOfWork.CashSessionsWrite.AddAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(session.PublicId);
    }
}
