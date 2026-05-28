using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExamplesFromSql;
// PATRÓN — Query SQL hardcoded en el handler cuando la lógica no se puede expresar con LINQ.
// · ISqlQueryRepository<T>.FromSqlAsync con FormattableString definido en el handler — no concatenar strings.
// · Diferencia con ExecuteSqlWithResult: el SQL lo define el handler (no el caller), garantizando seguridad.
// · Usar cuando la query analítica es fija y conocida en tiempo de diseño.
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
