using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleSummary;

// PATRÓN — Obtiene un resumen liviano del aggregate Example con estadísticas calculadas sobre sus Items.
//   PublicId identifica el aggregate; la query carga los hijos para calcular conteos sin exponerlos como colección.
//   Contrato de respuesta: Result<GetExampleSummaryDto>, o Result.Failure(NotFound) si no existe.
public record GetExampleSummaryQuery(Guid PublicId) : IRequest<Result<GetExampleSummaryDto>>;
