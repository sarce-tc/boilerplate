using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Application.Models;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExamplesPaginated;
// PATRÓN — Obtener colección paginada (offset) con metadatos para navegación del cliente.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IReadRepository<Example> (Application.Contracts.Persistence.EF): ejecuta
//     GetListPaginatedAsync que emite COUNT(*) + SELECT LIMIT/OFFSET y devuelve PagedResult<Example>.
//   · mapper — IMapper (AutoMapper): proyecta IEnumerable<Example> → IEnumerable<GetExamplesPaginatedDto>
//     antes de reconstruir el PagedResult con los metadatos originales.
public class GetExamplesPaginatedQueryHandler(
    IReadRepository<Example> readRepository,
    IMapper mapper
) : IRequestHandler<GetExamplesPaginatedQuery, Result<PagedResult<GetExamplesPaginatedDto>>>
{
    public async Task<Result<PagedResult<GetExamplesPaginatedDto>>> Handle(GetExamplesPaginatedQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await readRepository.GetListPaginatedAsync(
            request.CurrentPage,
            request.PageSize,
            cancellationToken: cancellationToken);

        var mappedResults = mapper.Map<IEnumerable<GetExamplesPaginatedDto>>(pagedResult.Results);

        var result = new PagedResult<GetExamplesPaginatedDto>(
            mappedResults,
            pagedResult.RowsCount,
            pagedResult.CurrentPage,
            pagedResult.PageSize);

        return Result<PagedResult<GetExamplesPaginatedDto>>.Success(result);
    }
}
