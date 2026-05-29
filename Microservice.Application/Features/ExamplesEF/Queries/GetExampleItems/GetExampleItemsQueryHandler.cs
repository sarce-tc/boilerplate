using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleItems;
// PATRÓN — Query que devuelve solo la colección hija usando generic-first.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IReadRepository<Example> (Application.Contracts.Persistence.EF): carga el aggregate con
//     includeProperties:[e => e.Items] para acceder a la colección sin método específico en el repositorio.
//   · mapper — IMapper (AutoMapper): proyecta ICollection<ExampleItem> → IEnumerable<GetExampleItemDto>
//     mapeando solo la colección hija, no el aggregate raíz completo.
public sealed class GetExampleItemsQueryHandler(
    IReadRepository<Example> readRepository,
    IMapper mapper
) : IRequestHandler<GetExampleItemsQuery, Result<IEnumerable<GetExampleItemDto>>>
{
    public async Task<Result<IEnumerable<GetExampleItemDto>>> Handle(
        GetExampleItemsQuery request,
        CancellationToken cancellationToken)
    {
        var example = await readRepository.GetEntityAsync(
            x => x.PublicId == request.ExamplePublicId,
            includeProperties: [e => e.Items],
            cancellationToken: cancellationToken);

        if (example is null)
            return Result<IEnumerable<GetExampleItemDto>>.Failure(Error.NotFound($"Example {request.ExamplePublicId} not found."));

        return Result<IEnumerable<GetExampleItemDto>>.Success(mapper.Map<IEnumerable<GetExampleItemDto>>(example.Items));
    }
}
