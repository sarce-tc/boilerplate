using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Queries.CountExamples;
// PATRÓN — Cuenta el total de aggregates Example sin materializar entidades.
//   No tiene propiedades de filtro; devuelve el conteo global de la tabla.
//   Contrato de respuesta: Result<int> con el número total de registros.
public record CountExamplesQuery : IRequest<Result<int>>;
