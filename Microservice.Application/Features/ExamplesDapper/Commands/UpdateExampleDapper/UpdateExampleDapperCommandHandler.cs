using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Application.Features.ExamplesDapper.Commands.UpdateExampleDapper;
// PATRÓN — Carga el aggregate Example, aplica domain methods fuera de TX y persiste los cambios via Dapper UoW.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IExampleReadRepository: superficie de lectura Dapper del aggregate
//     Example; localiza el registro por PublicId fuera de la transacción para minimizar el alcance del TX.
//   · unitOfWork — IUnitOfWork (Dapper): gestiona la transacción explícita y expone
//     ExamplesWrite para delegar la escritura al repositorio Dapper correspondiente.
public sealed class UpdateExampleDapperCommandHandler(
    IExampleReadRepository readRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateExampleDapperCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        UpdateExampleDapperCommand request, CancellationToken cancellationToken)
    {
        var example = await readRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);
        if (example is null)
            return Result<Guid>.Failure(Error.NotFound($"Example with PublicId '{request.PublicId}' was not found."));

        if (request.Name is not null)
            example.UpdateName(request.Name);

        if (request.Description is not null)
            example.UpdateDescription(string.IsNullOrWhiteSpace(request.Description) ? null : request.Description);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await unitOfWork.ExamplesWrite.UpdateAsync(example, cancellationToken);
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
