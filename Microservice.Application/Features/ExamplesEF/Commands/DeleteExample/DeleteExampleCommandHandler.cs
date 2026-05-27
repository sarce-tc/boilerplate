using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Commands.DeleteExample;

// PATRÓN — Eliminar un aggregate individual con verificación de existencia previa.
// ── Decisiones de diseño que debe replicar para nuevas entidades ─────────
//   · GetEntityAsync sin includeProperties — los hijos no necesitan cargarse; el FK CASCADE
//     configurado en OnModelCreating (ExampleDbContext) los elimina a nivel de base de datos.
//   · La entidad se carga para poder devolver NotFound explícito si no existe.
//     Si no necesitas ese feedback, usar directamente DeleteManyExamplesCommandHandler
//     con un array de un solo elemento (un DELETE WHERE sin carga de entidad).
//   · Delete() marca la entidad como Deleted en el ChangeTracker; SaveChangesAsync la persiste.
// ── Cuándo aplicar este patrón vs DeleteManyExamplesCommandHandler ────────
//   Usar este patrón cuando necesitas NotFound controlado (Result.Failure) o cuando
//   debes ejecutar lógica de dominio antes de eliminar (ej. entity.Deactivate() previo).
//   Para eliminar múltiples registros sin lógica previa, usar DeleteManyExamplesCommandHandler.
public sealed class DeleteExampleCommandHandler(
    IReadRepository<Example> readRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteExampleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(DeleteExampleCommand request, CancellationToken cancellationToken)
    {
        var example = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            cancellationToken: cancellationToken);

        if (example is null)
            return Result<Guid>.Failure(Error.NotFound($"Example {request.PublicId} not found."));

        unitOfWork.ExamplesWrite.Delete(example);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(request.PublicId);
    }
}
