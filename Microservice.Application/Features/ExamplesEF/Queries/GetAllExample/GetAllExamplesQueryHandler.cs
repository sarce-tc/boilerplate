using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetAllExample;
// PATRÓN — Obtener colección completa sin paginación con proyección a DTO.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IReadRepository<Example> (Application.Contracts.Persistence.EF): ejecuta GetListAsync
//     con predicado y disableTracking:true (default) para materializar todos los aggregates sin overhead de tracking.
//   · mapper — IMapper (AutoMapper): proyecta IEnumerable<Example> → IEnumerable<GetAllExamplesDto>.
public sealed class GetAllExamplesQueryHandler(
    IReadRepository<Example> readRepository,
    IMapper mapper
    ) : IRequestHandler<GetAllExamplesQuery, Result<IEnumerable<GetAllExamplesDto>>>
{
    public async Task<Result<IEnumerable<GetAllExamplesDto>>> Handle(GetAllExamplesQuery request, CancellationToken cancellationToken)
    {
        // Include de hijos: EF carga Items genéricamente; AutoMapper proyecta la colección.
        var data = mapper.Map<IEnumerable<GetAllExamplesDto>>(
            await readRepository.GetListAsync(
                x => x.Id > 0,
                includeProperties: [x => x.Items],
                cancellationToken: cancellationToken));

        return Result<IEnumerable<GetAllExamplesDto>>.Success(data);
    }
}
