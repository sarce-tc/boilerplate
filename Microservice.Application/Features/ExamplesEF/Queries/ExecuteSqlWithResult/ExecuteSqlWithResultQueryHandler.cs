using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.ExecuteSqlWithResult;
// PATRÓN — Ejecutar SQL SELECT arbitrario y mapear resultados a un DTO.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · sqlQueryRepository — ISqlQueryRepository<Example> (Application.Contracts.Persistence.EF): ejecuta el
//     FormattableString del query mediante FromSqlAsync y materializa los resultados en entidades Example.
//   · mapper — IMapper (AutoMapper): proyecta IEnumerable<Example> → IReadOnlyList<ExecuteSqlWithResultDto>.
public sealed class ExecuteSqlWithResultQueryHandler(
    ISqlQueryRepository<Example> sqlQueryRepository,
    IMapper mapper
    ) : IRequestHandler<ExecuteSqlWithResultQuery, Result<IReadOnlyList<ExecuteSqlWithResultDto>>>
{
    public async Task<Result<IReadOnlyList<ExecuteSqlWithResultDto>>> Handle(ExecuteSqlWithResultQuery request, CancellationToken cancellationToken)
    {
        var examples = await sqlQueryRepository.FromSqlAsync(request.Sql, cancellationToken);
        var data = mapper.Map<IReadOnlyList<ExecuteSqlWithResultDto>>(examples);
        return Result<IReadOnlyList<ExecuteSqlWithResultDto>>.Success(data);
    }
}
