using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;

namespace Microservice.Application.Features.ExamplesDapper.Queries.SearchExamplesByNameDapper;
// PATRÓN — Busca Examples por nombre (ILike) y los devuelve CON sus hijos.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IExampleReadRepository: SearchByNameWithItemsAsync proyecta las
//     coincidencias + sus ExampleItem por JOIN + multi-mapping a DTOs (sin AutoMapper).
public sealed class SearchExamplesByNameDapperQueryHandler(
    IExampleReadRepository readRepository) : IRequestHandler<SearchExamplesByNameDapperQuery, Result<IEnumerable<SearchExamplesByNameDto>>>
{
    public async Task<Result<IEnumerable<SearchExamplesByNameDto>>> Handle(
        SearchExamplesByNameDapperQuery request, CancellationToken cancellationToken)
    {
        var results = await readRepository.SearchByNameWithItemsAsync(request.Name, cancellationToken);
        return Result<IEnumerable<SearchExamplesByNameDto>>.Success(results);
    }
}
