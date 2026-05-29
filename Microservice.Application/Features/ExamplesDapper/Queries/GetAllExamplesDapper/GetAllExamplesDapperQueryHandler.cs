using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;

namespace Microservice.Application.Features.ExamplesDapper.Queries.GetAllExamplesDapper;
// PATRÓN — Retorna todos los registros del aggregate Example proyectados a DTO de respuesta.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IExampleReadRepository: superficie de lectura Dapper del aggregate
//     Example; expone GetAllAsync y operaciones de consulta específicas del dominio.
//     Dapper hidrata las entidades mapeando columnas snake_case → PascalCase via
//     MatchNamesWithUnderscores (configurado globalmente en Infrastructure).
//   · mapper — IMapper (AutoMapper): proyecta la colección de entidades Example
//     hacia GetAllExamplesDapperDto; el perfil de mapeo vive en MappingProfile.
public sealed class GetAllExamplesDapperQueryHandler(
    IExampleReadRepository readRepository,
    IMapper mapper) : IRequestHandler<GetAllExamplesDapperQuery, Result<IEnumerable<GetAllExamplesDto>>>
{
    public async Task<Result<IEnumerable<GetAllExamplesDto>>> Handle(
        GetAllExamplesDapperQuery request, CancellationToken cancellationToken)
    {
        var data = mapper.Map<IEnumerable<GetAllExamplesDto>>(
            await readRepository.GetAllAsync(cancellationToken));

        return Result<IEnumerable<GetAllExamplesDto>>.Success(data);
    }
}
