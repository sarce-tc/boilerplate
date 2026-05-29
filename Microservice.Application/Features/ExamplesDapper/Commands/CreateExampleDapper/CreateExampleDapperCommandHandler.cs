using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesDapper.Commands.CreateExampleDapper;
// PATRÓN — Persiste un nuevo aggregate Example via Dapper UoW con transacción explícita y retorna su PublicId.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · unitOfWork — IUnitOfWork (Dapper): gestiona la transacción explícita y expone
//     ExamplesWrite para delegar la escritura al repositorio Dapper correspondiente.
public sealed class CreateExampleDapperCommandHandler(
    IUnitOfWork unitOfWork) : IRequestHandler<CreateExampleDapperCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateExampleDapperCommand request, CancellationToken cancellationToken)
    {
        var entity = new Example(request.Name, request.Description);

        // PATRÓN — hijos via domain method: AddItem valida invariantes (activo, label
        // único, quantity > 0) ANTES de abrir la transacción. El write repo persiste
        // el aggregate + sus Items atómicamente dentro del mismo TX (ver AddAsync).
        if (request.Items is { Count: > 0 })
            foreach (var item in request.Items)
                entity.AddItem(item.Label, item.Quantity);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var created = await unitOfWork.ExamplesWrite.AddAsync(entity, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return Result<Guid>.Success(created.PublicId);
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
