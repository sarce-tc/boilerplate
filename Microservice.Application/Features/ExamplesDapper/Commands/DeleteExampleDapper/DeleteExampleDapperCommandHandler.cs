using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Application.Features.ExamplesDapper.Commands.DeleteExampleDapper;
// PATRÓN — Localiza el aggregate Example por PublicId y lo elimina via Dapper UoW con transacción explícita.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IExampleReadRepository: superficie de lectura Dapper del aggregate
//     Example; localiza el registro por PublicId fuera de la transacción para obtener el id interno.
//   · unitOfWork — IUnitOfWork (Dapper): gestiona la transacción explícita y expone
//     ExamplesWrite para delegar la eliminación al repositorio Dapper correspondiente.
public sealed class DeleteExampleDapperCommandHandler(
    IExampleReadRepository readRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteExampleDapperCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        DeleteExampleDapperCommand request, CancellationToken cancellationToken)
    {
        var example = await readRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);
        if (example is null)
            return Result<Guid>.Failure(Error.NotFound($"Example with PublicId '{request.PublicId}' was not found."));

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await unitOfWork.ExamplesWrite.DeleteAsync(example.Id, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return Result<Guid>.Success(example.PublicId);
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
