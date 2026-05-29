using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;
using Microservice.Application.Models;

namespace Microservice.Application.Features.ExamplesDapper.Queries.GetExamplesPaginatedDapper;
// PATRÓN — Retorna una página del aggregate Example CON sus hijos y metadatos de navegación.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IExampleReadRepository: GetPaginatedWithItemsAsync pagina el PADRE
//     (LIMIT/OFFSET en subconsulta) y carga los hijos de esa página por JOIN + multi-mapping,
//     devolviendo PagedResult<DTO> ya proyectado (sin AutoMapper, no hidrata el dominio).
public sealed class GetExamplesPaginatedDapperQueryHandler(
    IExampleReadRepository readRepository) : IRequestHandler<GetExamplesPaginatedDapperQuery, Result<PagedResult<GetExamplesPaginatedDto>>>
{
    public async Task<Result<PagedResult<GetExamplesPaginatedDto>>> Handle(
        GetExamplesPaginatedDapperQuery request, CancellationToken cancellationToken)
    {
        var paged = await readRepository.GetPaginatedWithItemsAsync(
            request.CurrentPage, request.PageSize, cancellationToken);

        return Result<PagedResult<GetExamplesPaginatedDto>>.Success(paged);
    }
}
