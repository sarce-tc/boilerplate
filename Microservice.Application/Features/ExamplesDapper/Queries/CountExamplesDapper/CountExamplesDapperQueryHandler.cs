using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Application.Features.ExamplesDapper.Queries.CountExamplesDapper;
// PATRÓN — Retorna el conteo total de registros del aggregate Example via Dapper.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IExampleReadRepository: superficie de lectura Dapper del aggregate
//     Example; expone CountAsync para obtener el total de registros sin cargar entidades.
public sealed class CountExamplesDapperQueryHandler(
    IExampleReadRepository readRepository) : IRequestHandler<CountExamplesDapperQuery, Result<int>>
{
    public async Task<Result<int>> Handle(
        CountExamplesDapperQuery request, CancellationToken cancellationToken)
    {
        var count = await readRepository.CountAsync(cancellationToken);
        return Result<int>.Success(count);
    }
}
