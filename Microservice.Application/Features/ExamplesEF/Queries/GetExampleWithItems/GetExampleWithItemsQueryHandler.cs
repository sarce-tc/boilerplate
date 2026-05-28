using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleWithItems;
// PATRÓN — Query con eager-loading de colección hija usando generic-first.
// · IReadRepository<Example> + includeProperties:[e => e.Items] carga el aggregate
//   con sus hijos en una sola ida a la BD sin crear un método específico en el repositorio.
// · Solo crear IExampleReadRepository cuando el query necesite lógica que no existe en
//   GetEntityAsync (ej. ILike case-insensitive, ThenInclude anidado con filtro).
// · AutoMapper resuelve Example → GetExampleWithItemsDto incluyendo Items automáticamente.
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
