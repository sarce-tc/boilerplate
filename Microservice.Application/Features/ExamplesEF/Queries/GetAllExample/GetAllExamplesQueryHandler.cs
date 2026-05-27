using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetAllExample
{
    // PATRÓN — Obtener colección completa sin paginación con proyección a DTO.
    // ── Decisiones de diseño que debe replicar para nuevas entidades ─────────
    //   · Generic-first: inyectar IReadRepository<T> directamente porque GetListAsync
    //     existe en la superficie genérica.
    //   · Sin paginación — solo aplicar cuando la colección está acotada por diseño
    //     (catálogos, listas de referencia). Para colecciones no acotadas, usar
    //     GetExamplesPaginatedQueryHandler (ver §7 CLAUDE.md: paginación obligatoria).
    //   · GetListAsync con predicado y disableTracking:true (default) — queries de lectura
    //     pura nunca necesitan tracking.
    public class GetAllExamplesQueryHandler(
        IReadRepository<Example> readRepository,
        IMapper mapper
        ) : IRequestHandler<GetAllExamplesQuery, Result<IEnumerable<GetAllExamplesDto>>>
    {
        public async Task<Result<IEnumerable<GetAllExamplesDto>>> Handle(GetAllExamplesQuery request, CancellationToken cancellationToken)
        {
            var data = mapper.Map<IEnumerable<GetAllExamplesDto>>(
                await readRepository.GetListAsync(x => x.Id > 0, cancellationToken: cancellationToken));

            return Result<IEnumerable<GetAllExamplesDto>>.Success(data);
        }
    }
}
