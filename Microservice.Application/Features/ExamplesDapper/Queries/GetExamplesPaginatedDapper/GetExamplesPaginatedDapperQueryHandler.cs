using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs;
using Microservice.Application.Models;

namespace Microservice.Application.Features.ExamplesDapper.Queries.GetExamplesPaginatedDapper;
// PATRÓN — Retorna una página de registros del aggregate Example con metadatos de navegación.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IExampleReadRepository: superficie de lectura Dapper del aggregate
//     Example; expone GetListPaginatedAsync que emite COUNT(*) + SELECT LIMIT/OFFSET
//     en dos queries y retorna PagedResult<T> con RowsCount, CurrentPage, PageSize y PageCount.
//   · mapper — IMapper (AutoMapper): proyecta cada Example del resultado hacia
//     GetExamplesPaginatedDapperDto; el perfil de mapeo vive en MappingProfile.
public sealed class GetExamplesPaginatedDapperQueryHandler(
    IExampleReadRepository readRepository,
    IMapper mapper) : IRequestHandler<GetExamplesPaginatedDapperQuery, Result<PagedResult<GetExamplesPaginatedDapperDto>>>
{
    public async Task<Result<PagedResult<GetExamplesPaginatedDapperDto>>> Handle(
        GetExamplesPaginatedDapperQuery request, CancellationToken cancellationToken)
    {
        var paged = await readRepository.GetListPaginatedAsync(
            request.CurrentPage, request.PageSize, cancellationToken);

        var mapped = new PagedResult<GetExamplesPaginatedDapperDto>(
            mapper.Map<IEnumerable<GetExamplesPaginatedDapperDto>>(paged.Results),
            paged.RowsCount,
            paged.CurrentPage,
            paged.PageSize);

        return Result<PagedResult<GetExamplesPaginatedDapperDto>>.Success(mapped);
    }
}
