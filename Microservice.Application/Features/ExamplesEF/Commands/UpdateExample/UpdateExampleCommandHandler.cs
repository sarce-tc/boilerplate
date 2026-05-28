using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Commands.UpdateExample;

// PATRÓN — Actualizar aggregate completo (scalar + gestión de hijos) con change tracking.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IReadRepository<Example> (Application.Contracts.Persistence.EF): carga el aggregate
//     con disableTracking:false e includeProperties Items para que EF registre cambios en hijos automáticamente.
//   · unitOfWork — IUnitOfWork (Application.Contracts.Persistence.EF): expone ExamplesWrite.Update para
//     marcar el aggregate como modificado y SaveChangesAsync para confirmar scalars + hijos en una TX implícita.
// ── Decisiones de diseño de referencia ────────────────────────────────────
//   · includeProperties:[e => e.Items] — cargar la colección hija es obligatorio cuando
//     el handler puede añadir, quitar o transicionar hijos, para que:
//       a) los domain methods puedan validar invariantes sobre el estado actual de la colección.
//       b) el snapshot change-tracker de EF detecte las diferencias al hacer SaveChanges.
//   · disableTracking:false — la entidad queda en tracked state para que EF genere
//     automáticamente INSERT/UPDATE/DELETE sobre los hijos modificados.
//   · Los cambios se aplican exclusivamente via domain methods (UpdateName, AddItem, etc.);
//     nunca asignar propiedades directamente desde el handler.
//   · Un único SaveChangesAsync confirma scalars + hijos en una TX implícita.
// ── Cuándo aplicar este patrón ───────────────────────────────────────────
//   Endpoint PUT donde el caller puede enviar scalar + operaciones sobre hijos en el mismo request.
//   Si el endpoint solo actualiza campos scalares sin hijos, ver UpdateExampleFieldsCommandHandler.
public sealed class UpdateExampleCommandHandler(
    IReadRepository<Example> readRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateExampleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateExampleCommand request, CancellationToken cancellationToken)
    {
        var example = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            includeProperties: [e => e.Items],
            disableTracking: false,
            cancellationToken: cancellationToken);

        if (example is null)
            return Result<Guid>.Failure(Error.NotFound($"Example {request.PublicId} not found."));

        if (request.Name is not null)
            example.UpdateName(request.Name);

        if (request.Description is not null)
            example.UpdateDescription(string.IsNullOrWhiteSpace(request.Description) ? null : request.Description);

        if (request.AddItems is { Count: > 0 })
            foreach (var item in request.AddItems)
                example.AddItem(item.Label, item.Quantity);

        if (request.RemoveItemIds is { Count: > 0 })
            foreach (var itemId in request.RemoveItemIds)
                example.RemoveItem(itemId);

        if (request.CompleteItemIds is { Count: > 0 })
            foreach (var itemId in request.CompleteItemIds)
                example.CompleteItem(itemId);

        unitOfWork.ExamplesWrite.Update(example);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(example.PublicId);
    }
}
