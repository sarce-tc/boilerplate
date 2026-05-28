using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Services;

namespace Microservice.Application.Features.ExamplesEF.Commands.ActivateExample;

// PATRÓN — Transicionar el estado del aggregate Example de Inactive a Active via domain method.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · exampleService — IExampleService (Application.Services): encapsula el lookup estándar por publicId con
//     change-tracking activo (FindTrackedAsync) para que EF detecte el cambio de estado sin includeProperties.
//   · unitOfWork — IUnitOfWork (Application.Contracts.Persistence.EF): expone ExamplesWrite.Update para marcar
//     el aggregate como modificado y SaveChangesAsync para confirmar el cambio de estado en la TX implícita.
public sealed class ActivateExampleCommandHandler(
    IExampleService exampleService,
    IUnitOfWork unitOfWork
) : IRequestHandler<ActivateExampleCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(
        ActivateExampleCommand request, CancellationToken cancellationToken)
    {
        var example = await exampleService.FindTrackedAsync(request.PublicId, cancellationToken);

        if (example is null)
            return Result<Guid>.Failure(Error.NotFound($"Example {request.PublicId} not found."));

        // DomainException si ya está Active → GlobalExceptionHandler → HTTP 409
        example.Activate();

        unitOfWork.ExamplesWrite.Update(example);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(example.PublicId);
    }
}
