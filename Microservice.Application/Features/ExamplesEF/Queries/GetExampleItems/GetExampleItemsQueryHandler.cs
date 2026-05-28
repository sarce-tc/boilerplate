using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleItems;
// PATRÓN — Query que devuelve solo la colección hija usando generic-first.
// · IReadRepository<Example> + includeProperties:[e => e.Items] — sin método específico.
// · Se mapea únicamente example.Items (no el aggregate completo) para devolver
//   la colección plana IEnumerable<GetExampleItemDto>.
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
