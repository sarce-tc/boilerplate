using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;

namespace Microservice.Application.Features.ExamplesDapper.Queries.GetExampleByPublicIdDapper;
// PATRÓN — Recupera un Example por PublicId CON sus hijos (read model con items).
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IExampleReadRepository: GetByPublicIdWithItemsAsync proyecta el
//     aggregate + sus ExampleItem por JOIN + multi-mapping a un DTO (no hidrata el dominio
//     ni usa AutoMapper). Items es read-only en el dominio, por eso el read va a DTO.
public sealed class GetExampleByPublicIdDapperQueryHandler(
    IExampleReadRepository readRepository) : IRequestHandler<GetExampleByPublicIdDapperQuery, Result<GetExampleByPublicIdDto>>
{
    public async Task<Result<GetExampleByPublicIdDto>> Handle(
        GetExampleByPublicIdDapperQuery request, CancellationToken cancellationToken)
    {
        var dto = await readRepository.GetByPublicIdWithItemsAsync(request.PublicId, cancellationToken);
        if (dto is null)
            return Result<GetExampleByPublicIdDto>.Failure(Error.NotFound($"Example with PublicId '{request.PublicId}' was not found."));

        return Result<GetExampleByPublicIdDto>.Success(dto);
    }
}
