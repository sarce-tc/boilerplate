using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;
using System.Linq.Expressions;

namespace Microservice.Application.Features.ExamplesEF.Commands.UpdateExampleFields;

// PATRÓN — Actualizar uno o varios campos scalares del aggregate con semántica PATCH (mínimo SQL).
// ── Decisiones de diseño que debe replicar para nuevas entidades ─────────
//   · disableTracking:true (default) — no se necesita tracking completo porque no hay hijos.
//   · UpdateFields(entity, [x => x.Campo1, x => x.Campo2]) hace Attach + marca solo esas
//     columnas como Modified; el UPDATE resultante toca únicamente esas columnas.
//   · Los campos a actualizar se acumulan en una lista y se pasan a UpdateFields de una vez;
//     esto permite que el caller envíe cualquier combinación sin múltiples round-trips.
//   · Domain methods aplican reglas antes de marcar campos: nunca asignar la propiedad directamente.
// ── Cuándo aplicar este patrón vs UpdateExampleCommandHandler ────────────
//   Usar este patrón cuando el endpoint es PATCH y el caller puede enviar 1-N campos
//   scalares sin necesidad de gestionar la colección de hijos.
//   Si el endpoint incluye gestión de hijos, usar UpdateExampleCommandHandler (PUT).
public sealed class UpdateExampleFieldsCommandHandler(
    IReadRepository<Example> readRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateExampleFieldsCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateExampleFieldsCommand request, CancellationToken cancellationToken)
    {
        var example = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            cancellationToken: cancellationToken);

        if (example is null)
            return Result<Guid>.Failure(Error.NotFound($"Example {request.PublicId} not found."));

        var fieldsToUpdate = new List<Expression<Func<Example, object>>>();

        if (request.Name is not null)
        {
            example.UpdateName(request.Name);
            fieldsToUpdate.Add(x => x.Name);
        }
        if (request.Description is not null)
        {
            example.UpdateDescription(string.IsNullOrWhiteSpace(request.Description) ? null : request.Description);
            fieldsToUpdate.Add(x => x.Description!);
        }

        unitOfWork.ExamplesWrite.UpdateFields(example, [.. fieldsToUpdate]);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(example.PublicId);
    }
}
