using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.ExecuteSqlWithResult;
// PATRÓN — Ejecutar SQL SELECT arbitrario y mapear resultados a un DTO.
// · ISqlQueryRepository<T>.FromSqlAsync — recibe FormattableString del request y materializa entidades.
// · AutoMapper convierte las entidades al DTO de respuesta.
// · Usar cuando la query es tan compleja que IReadRepository<T> no la puede expresar con predicados.
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
