using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleWithProjection;
// PATRÓN — Obtiene un único aggregate Example proyectando solo los campos necesarios en SQL.
//   PublicId identifica el aggregate a recuperar con proyección de columnas reducida.
//   Contrato de respuesta: Result<GetExampleWithProjectionDto>, o Result.Failure(NotFound) si no existe.
public record GetExampleWithProjectionQuery(
    Guid PublicId
) : IRequest<Result<GetExampleWithProjectionDto>>;
