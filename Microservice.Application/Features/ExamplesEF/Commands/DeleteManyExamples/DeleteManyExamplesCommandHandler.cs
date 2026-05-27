using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Commands.DeleteManyExamples;

// PATRÓN — Eliminar múltiples registros en un único statement SQL sin cargar entidades.
// ── Decisiones de diseño que debe replicar para nuevas entidades ─────────
//   · Generic-first: se usa IUnitOfWork.WriteRepository (genérico) porque DeleteManyAsync
//     existe en la superficie genérica. No se necesita IExampleWriteRepository.
//   · DeleteManyAsync traduce el predicado a DELETE WHERE via ExecuteDeleteAsync (EF Bulk Ops).
//     No materializa entidades → mínimo overhead de memoria y red.
//   · Los hijos se eliminan por el CASCADE de base de datos, no por EF ChangeTracker.
//   · Devuelve el conteo de registros eliminados, no NotFound — si un PublicId no existe
//     simplemente no se cuenta; usar DeleteExampleCommandHandler si necesitas NotFound explícito.
// ── Cuándo aplicar este patrón ───────────────────────────────────────────
//   Endpoint DELETE bulk donde el caller provee N ids y se necesita mínimo overhead.
//   No usar cuando debes ejecutar domain methods antes de eliminar (requiere carga individual).
public sealed class DeleteManyExamplesCommandHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteManyExamplesCommand, Result<int>>
{
    public async Task<Result<int>> Handle(DeleteManyExamplesCommand request, CancellationToken cancellationToken)
    {
        var deletedCount = await unitOfWork.WriteRepository.DeleteManyAsync(
            x => request.PublicIds.Contains(x.PublicId),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(deletedCount);
    }
}
