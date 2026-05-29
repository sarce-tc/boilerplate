using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;

namespace Microservice.Application.Features.ExamplesDapper.Queries.GetAllExamplesDapper;
// PATRÓN — Retorna todos los registros del aggregate Example CON sus hijos.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IExampleReadRepository: GetAllWithItemsAsync proyecta cada aggregate
//     + sus ExampleItem por JOIN + multi-mapping a DTOs (sin AutoMapper, no hidrata el dominio).
public sealed class GetAllExamplesDapperQueryHandler(
    IExampleReadRepository readRepository) : IRequestHandler<GetAllExamplesDapperQuery, Result<IEnumerable<GetAllExamplesDto>>>
{
    public async Task<Result<IEnumerable<GetAllExamplesDto>>> Handle(
        GetAllExamplesDapperQuery request, CancellationToken cancellationToken)
    {
        var data = await readRepository.GetAllWithItemsAsync(cancellationToken);
        return Result<IEnumerable<GetAllExamplesDto>>.Success(data);
    }
}
