using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleWithItems;
// PATRÓN — Query con eager-loading de colección hija usando generic-first.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IReadRepository<Example> (Application.Contracts.Persistence.EF): carga el aggregate
//     con includeProperties:[e => e.Items] para materializar root + hijos en una sola consulta SQL.
//   · mapper — IMapper (AutoMapper): proyecta Example (con Items) → GetExampleWithItemsDto incluyendo la
//     colección hija mapeada a IEnumerable<GetExampleItemDto>.
public sealed class GetExampleWithItemsQueryHandler(
    IReadRepository<Example> readRepository,
    IMapper mapper
) : IRequestHandler<GetExampleWithItemsQuery, Result<GetExampleWithItemsDto>>
{
    public async Task<Result<GetExampleWithItemsDto>> Handle(
        GetExampleWithItemsQuery request,
        CancellationToken cancellationToken)
    {
        var example = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            includeProperties: [e => e.Items],
            cancellationToken: cancellationToken);

        if (example is null)
            return Result<GetExampleWithItemsDto>.Failure(Error.NotFound($"Example {request.PublicId} not found."));

        return Result<GetExampleWithItemsDto>.Success(mapper.Map<GetExampleWithItemsDto>(example));
    }
}
