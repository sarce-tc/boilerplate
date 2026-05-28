using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleById;
// PATRÓN — Obtener un aggregate por su PK interna con proyección a DTO.
// ── Decisiones de diseño de referencia ────────────────────────────────────
//   · Generic-first: inyectar IReadRepository<T> directamente porque FindAsync
//     existe en la superficie genérica. No se necesita IMyEntityReadRepository.
//   · FindAsync usa el cache de primer nivel del DbContext (identity map) si la entidad
//     ya fue cargada en la misma solicitud, evitando un round-trip adicional.
//   · AutoMapper proyecta la entidad al DTO de salida.
//   · El handler devuelve NotFound si la entidad no existe (Result pattern, no excepción).
// ── Cuándo crear IMyEntityReadRepository en lugar de IReadRepository<T> ──
//   Solo cuando el query necesita lógica que no existe en la superficie genérica,
//   por ejemplo ILike case-insensitive. El eager-loading de hijos no justifica la
//   interfaz específica: usar GetEntityAsync con includeProperties:[e => e.Children].
public sealed class GetExampleByIdQueryHandler(
    IReadRepository<Example> readRepository,
    IMapper mapper
    ) : IRequestHandler<GetExampleByIdQuery, Result<GetExampleByIdDto>>
{
    public async Task<Result<GetExampleByIdDto>> Handle(GetExampleByIdQuery request, CancellationToken cancellationToken)
    {
        var example = await readRepository.FindAsync(request.Id, cancellationToken);

        if (example is null)
            return Result<GetExampleByIdDto>.Failure(Error.NotFound("Ejemplo no encontrado"));

        return Result<GetExampleByIdDto>.Success(mapper.Map<GetExampleByIdDto>(example));
    }
}
