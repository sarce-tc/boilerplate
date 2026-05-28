using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExamplesFromSql;
// PATRÓN — Query SQL hardcoded en el handler cuando la lógica no se puede expresar con LINQ.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · sqlQueryRepository — ISqlQueryRepository<Example> (Application.Contracts.Persistence.EF): ejecuta el
//     FormattableString definido en el handler (no por el caller) mediante FromSqlAsync, garantizando que
//     el SQL es seguro y conocido en tiempo de diseño.
//   · mapper — IMapper (AutoMapper): proyecta IEnumerable<Example> → IEnumerable<GetExamplesFromSqlDto>.
public sealed class GetExamplesFromSqlQueryHandler(
    ISqlQueryRepository<Example> sqlQueryRepository,
    IMapper mapper
    ) : IRequestHandler<GetExamplesFromSqlQuery, Result<IEnumerable<GetExamplesFromSqlDto>>>
{
    public async Task<Result<IEnumerable<GetExamplesFromSqlDto>>> Handle(
        GetExamplesFromSqlQuery request, 
        CancellationToken cancellationToken)
    {
        FormattableString sql = $"SELECT * FROM \"Examples\" WHERE \"Id\" > 0";

        var examples = await sqlQueryRepository.FromSqlAsync(sql, cancellationToken);
        var data = mapper.Map<IEnumerable<GetExamplesFromSqlDto>>(examples);

        return Result<IEnumerable<GetExamplesFromSqlDto>>.Success(data);
    }
}
