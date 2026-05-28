using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleItemByPublicId;
// PATRÓN — Query sobre un hijo específico usando generic-first.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IReadRepository<Example> (Application.Contracts.Persistence.EF): carga el aggregate con
//     includeProperties:[e => e.Items]; el filtro del hijo se realiza en memoria sobre example.Items.
//   · mapper — IMapper (AutoMapper): proyecta el ExampleItem encontrado → GetExampleItemDto.
public sealed class GetExampleItemByPublicIdQueryHandler(
    IReadRepository<Example> readRepository,
    IMapper mapper
) : IRequestHandler<GetExampleItemByPublicIdQuery, Result<GetExampleItemDto>>
{
    public async Task<Result<GetExampleItemDto>> Handle(
        GetExampleItemByPublicIdQuery request,
        CancellationToken cancellationToken)
    {
        var example = await readRepository.GetEntityAsync(
            x => x.PublicId == request.ExamplePublicId,
            includeProperties: [e => e.Items],
            cancellationToken: cancellationToken);

        if (example is null)
            return Result<GetExampleItemDto>.Failure(Error.NotFound($"Example {request.ExamplePublicId} not found."));

        var item = example.Items.FirstOrDefault(i => i.PublicId == request.ItemPublicId);

        if (item is null)
            return Result<GetExampleItemDto>.Failure(Error.NotFound($"Item {request.ItemPublicId} not found."));

        return Result<GetExampleItemDto>.Success(mapper.Map<GetExampleItemDto>(item));
    }
}
