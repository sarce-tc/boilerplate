using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleWithProjection;
// PATRÓN — Query con proyección de campos para un único aggregate (reduce columnas transferidas).
// ── Parámetros ────────────────────────────────────────────────────────────
//   · queryRepository — IQueryRepository<Example> (Application.Contracts.Persistence.EF): ejecuta GetEntityAsync
//     con selector lambda y predicado, emitiendo SELECT solo de las columnas del DTO sin materializar la entidad
//     completa ni invocar AutoMapper.
public sealed class GetExampleWithProjectionQueryHandler(
    IQueryRepository<Example> queryRepository
    ) : IRequestHandler<GetExampleWithProjectionQuery, Result<GetExampleWithProjectionDto>>
{
    public async Task<Result<GetExampleWithProjectionDto>> Handle(GetExampleWithProjectionQuery request, CancellationToken cancellationToken)
    {
        var result = await queryRepository.GetEntityAsync(x => new GetExampleWithProjectionDto(x.PublicId, x.Name, x.Description), x => x.PublicId == request.PublicId, cancellationToken);
        
        if (result is null)
            return Result<GetExampleWithProjectionDto>.Failure(Error.NotFound("Ejemplo no encontrado"));
        
        return Result<GetExampleWithProjectionDto>.Success(result);
    }
}
