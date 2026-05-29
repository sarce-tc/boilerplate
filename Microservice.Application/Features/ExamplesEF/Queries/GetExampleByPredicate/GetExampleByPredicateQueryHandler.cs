using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;
using System.Linq.Expressions;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleByPredicate;
// PATRÓN — Query de aggregate raíz CON hijos usando generic-first.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IReadRepository<Example> (Application.Contracts.Persistence.EF): ejecuta GetEntityAsync
//     con predicado lambda + includeProperties [x => x.Items] para eager-load de la colección de hijos.
//   · mapper — IMapper (AutoMapper): proyecta Example (con Items) → GetExampleByPredicateDto.
public sealed class GetExampleByPredicateQueryHandler(
    IReadRepository<Example> readRepository,
    IMapper mapper
    ) : IRequestHandler<GetExampleByPredicateQuery, Result<GetExampleByPredicateDto>>
{
    public async Task<Result<GetExampleByPredicateDto>> Handle(GetExampleByPredicateQuery request, CancellationToken cancellationToken)
    {
        var example = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            includeProperties: [x => x.Items],
            cancellationToken: cancellationToken);

        if (example is null)
            return Result<GetExampleByPredicateDto>.Failure(Error.NotFound("Ejemplo no encontrado"));

        return Result<GetExampleByPredicateDto>.Success(mapper.Map<GetExampleByPredicateDto>(example));
    }
}
