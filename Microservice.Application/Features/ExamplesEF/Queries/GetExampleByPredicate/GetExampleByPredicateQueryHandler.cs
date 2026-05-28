using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;
using System.Linq.Expressions;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleByPredicate;
// PATRÓN — Query de aggregate raíz sin hijos usando generic-first.
// · IReadRepository<T>.GetEntityAsync con predicado lambda — sin includeProperties porque
//   solo se necesita el aggregate raíz (no la colección Items).
// · Para cargar hijos, agregar includeProperties:[e => e.Items] al mismo método — no crear
//   un método específico en el repositorio.
public sealed class GetExampleByPredicateQueryHandler(
    IReadRepository<Example> readRepository,
    IMapper mapper
    ) : IRequestHandler<GetExampleByPredicateQuery, Result<GetExampleByPredicateDto>>
{
    public async Task<Result<GetExampleByPredicateDto>> Handle(GetExampleByPredicateQuery request, CancellationToken cancellationToken)
    {
        var example = await readRepository.GetEntityAsync(x => x.PublicId == request.PublicId, cancellationToken: cancellationToken);

        if (example is null)
            return Result<GetExampleByPredicateDto>.Failure(Error.NotFound("Ejemplo no encontrado"));

        return Result<GetExampleByPredicateDto>.Success(mapper.Map<GetExampleByPredicateDto>(example));
    }
}
