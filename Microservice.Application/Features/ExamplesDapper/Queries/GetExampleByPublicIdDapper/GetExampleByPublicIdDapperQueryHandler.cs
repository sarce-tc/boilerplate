using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;

namespace Microservice.Application.Features.ExamplesDapper.Queries.GetExampleByPublicIdDapper;
// PATRÓN — Recupera un único Example por su PublicId y lo proyecta a DTO de respuesta.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IExampleReadRepository: superficie de lectura Dapper del aggregate
//     Example; expone GetByPublicIdAsync para localizar el registro por su GUID público.
//   · mapper — IMapper (AutoMapper): proyecta la entidad Example hacia
//     GetExampleByPublicIdDapperDto; el perfil de mapeo vive en MappingProfile.
public sealed class GetExampleByPublicIdDapperQueryHandler(
    IExampleReadRepository readRepository,
    IMapper mapper) : IRequestHandler<GetExampleByPublicIdDapperQuery, Result<GetExampleByPublicIdDto>>
{
    public async Task<Result<GetExampleByPublicIdDto>> Handle(
        GetExampleByPublicIdDapperQuery request, CancellationToken cancellationToken)
    {
        var example = await readRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);
        if (example is null)
            return Result<GetExampleByPublicIdDto>.Failure(Error.NotFound($"Example with PublicId '{request.PublicId}' was not found."));

        return Result<GetExampleByPublicIdDto>.Success(mapper.Map<GetExampleByPublicIdDto>(example));
    }
}
