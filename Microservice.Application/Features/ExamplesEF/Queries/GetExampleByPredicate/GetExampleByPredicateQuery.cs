using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleByPredicate;
// PATRÓN — Recupera un único aggregate Example por su PublicId usando predicado genérico.
//   PublicId es el identificador público del aggregate a buscar.
//   Contrato de respuesta: Result<GetExampleByPredicateDto>, o Result.Failure(NotFound) si no existe.
public record GetExampleByPredicateQuery(
    Guid PublicId
) : IRequest<Result<GetExampleByPredicateDto>>;
