using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExamplesWithProjection;
// PATRÓN — Obtiene la colección de aggregates Example proyectando solo los campos necesarios en SQL.
//   No tiene propiedades de filtro; devuelve todos los aggregates con proyección de columnas reducida.
//   Contrato de respuesta: Result<IEnumerable<GetExamplesWithProjectionDto>> construido directamente en el selector EF.
public record GetExamplesWithProjectionQuery : IRequest<Result<IEnumerable<GetExamplesWithProjectionDto>>>;
