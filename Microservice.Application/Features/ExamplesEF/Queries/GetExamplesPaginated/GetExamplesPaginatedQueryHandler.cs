using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Application.Models;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExamplesPaginated
{
    // PATRÓN — Obtener colección paginada (offset) con metadatos para navegación del cliente.
    // ── Decisiones de diseño de referencia ────────────────────────────────────
    //   · Generic-first: inyectar IReadRepository<T> directamente porque GetListPaginatedAsync
    //     existe en la superficie genérica.
    //   · GetListPaginatedAsync emite COUNT(*) + SELECT LIMIT/OFFSET en dos queries.
    //     El resultado se envuelve en PagedResult<T> con RowsCount, CurrentPage, PageSize.
    //   · Paginación obligatoria para cualquier colección no acotada (ver §7 CLAUDE.md).
    //   · AutoMapper proyecta cada elemento al DTO de salida; PagedResult se reconstruye
    //     con los metadatos originales para que el cliente pueda navegar.
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
}
