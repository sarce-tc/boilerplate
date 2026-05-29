using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;

namespace Microservice.Application.Features.ExamplesDapper.Queries.SearchExamplesByNameDapper;
// PATRÓN — Busca Examples cuyo nombre coincida con el término provisto y los proyecta a colección de DTOs.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IExampleReadRepository: superficie de lectura Dapper del aggregate
//     Example; expone SearchByNameAsync para búsqueda case-insensitive (ILike) por nombre.
//   · mapper — IMapper (AutoMapper): proyecta la colección de entidades Example hacia
//     SearchExamplesByNameDapperDto; el perfil de mapeo vive en MappingProfile.
public sealed class SearchExamplesByNameDapperQueryHandler(
    IExampleReadRepository readRepository,
    IMapper mapper) : IRequestHandler<SearchExamplesByNameDapperQuery, Result<IEnumerable<SearchExamplesByNameDto>>>
{
    public async Task<Result<IEnumerable<SearchExamplesByNameDto>>> Handle(
        SearchExamplesByNameDapperQuery request, CancellationToken cancellationToken)
    {
        var results = await readRepository.SearchByNameAsync(request.Name, cancellationToken);
        return Result<IEnumerable<SearchExamplesByNameDto>>.Success(
            mapper.Map<IEnumerable<SearchExamplesByNameDto>>(results));
    }
}
