using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.ExecuteSqlWithResult;
// PATRÓN — Ejecuta un SELECT SQL arbitrario provisto por el caller y mapea los resultados a DTO.
//   Sql es el FormattableString parametrizado enviado desde el controller.
//   Contrato de respuesta: Result<IReadOnlyList<ExecuteSqlWithResultDto>> proyectado por AutoMapper.
public record ExecuteSqlWithResultQuery(
    FormattableString Sql
) : IRequest<Result<IReadOnlyList<ExecuteSqlWithResultDto>>>;
