using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExamplesFromSql;
// PATRÓN — Transporta el parámetro de filtro para la query SQL hardcoded en el handler.
//   Sql es el string de filtro provisto por el caller; el SQL real lo define el handler garantizando seguridad.
//   Contrato de respuesta: Result<IEnumerable<GetExamplesFromSqlDto>> proyectado por AutoMapper.
public record GetExamplesFromSqlQuery(
    string Sql
) : IRequest<Result<IEnumerable<GetExamplesFromSqlDto>>>;
