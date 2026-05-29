using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExamplesWithProjection;
// PATRÓN — Query con proyección de campos para colecciones (reduce columnas transferidas).
// ── Parámetros ────────────────────────────────────────────────────────────
//   · queryRepository — IQueryRepository<Example> (Application.Contracts.Persistence.EF): ejecuta GetListAsync
//     con un selector lambda que proyecta directamente a GetExamplesWithProjectionDto en SQL, sin AutoMapper,
//     emitiendo SELECT solo de las columnas del DTO para reducir el ancho de banda.
public sealed class GetExamplesWithProjectionQueryHandler(
    IQueryRepository<Example> queryRepository
    ) : IRequestHandler<GetExamplesWithProjectionQuery, Result<IEnumerable<GetExamplesWithProjectionDto>>>
{
    public async Task<Result<IEnumerable<GetExamplesWithProjectionDto>>> Handle(GetExamplesWithProjectionQuery request, CancellationToken cancellationToken)
    {
        // Items se proyectan inline: EF traduce el Select anidado a SQL (subconsulta),
        // sin Include ni materializar la entidad completa.
        var data = await queryRepository.GetListAsync(
            x => new GetExamplesWithProjectionDto(
                x.PublicId,
                x.Name,
                x.Description,
                x.Items.Select(i => new GetExampleItemProjectionDto(i.PublicId, i.Label, i.Quantity)).ToList()),
            cancellationToken: cancellationToken);
        return Result<IEnumerable<GetExamplesWithProjectionDto>>.Success(data);
    }
}
