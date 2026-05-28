using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Services;

namespace Microservice.Application.Features.ExamplesEF.Commands.ActivateExample;

// PATRÓN — Command que usa IExampleService (tracked) + IUnitOfWork sin includeProperties.
// ── Decisiones de diseño de referencia ────────────────────────────────────
//   · IExampleService.FindTrackedAsync — carga el aggregate con change-tracking activo
//     (disableTracking:false) sin importar IReadRepository<Example> directamente.
//     El handler solo necesita los campos escalares; no se cargan Items.
//   · example.Activate() lanza DomainException si el aggregate ya está Active.
//     GlobalExceptionHandler la mapea a HTTP 409 Conflict — no hay try-catch aquí.
//   · unitOfWork.ExamplesWrite.Update + SaveChangesAsync — patrón idéntico al de
//     UpdateExampleCommandHandler para scalars sin hijos.
// ── Cuándo usar IExampleService en lugar de IReadRepository<T> directamente ─
//   Cuando el lookup es estándar (por publicId, sin predicado personalizado).
//   Para commands que también necesitan hijos usar FindWithItemsTrackedAsync o
//   inyectar IReadRepository<T> con includeProperties y disableTracking:false.
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
