using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExamplesWithProjection;
// PATRÓN — Query con proyección de campos para colecciones (reduce columnas transferidas).
// · IQueryRepository<T>.GetListAsync con selector lambda — EF emite SELECT solo de las columnas del DTO.
// · No usa AutoMapper porque la proyección ocurre en SQL; el DTO se construye directamente en el selector.
// · Preferir sobre GetListAsync + Map cuando las entidades tienen muchas columnas y solo se necesitan algunas.
public sealed class GetExamplesWithProjectionQueryHandler(
    IQueryRepository<Example> queryRepository
    ) : IRequestHandler<GetExamplesWithProjectionQuery, Result<IEnumerable<GetExamplesWithProjectionDto>>>
{
    public async Task<Result<IEnumerable<GetExamplesWithProjectionDto>>> Handle(GetExamplesWithProjectionQuery request, CancellationToken cancellationToken)
    {
        var data = await queryRepository.GetListAsync(x => new GetExamplesWithProjectionDto(x.PublicId, x.Name, x.Description), cancellationToken: cancellationToken);
        return Result<IEnumerable<GetExamplesWithProjectionDto>>.Success(data);
    }
}
