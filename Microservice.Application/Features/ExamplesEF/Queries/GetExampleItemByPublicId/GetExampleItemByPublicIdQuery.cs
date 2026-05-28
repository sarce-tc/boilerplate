using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleItemByPublicId;
// PATRÓN — Obtiene un hijo ExampleItem específico dentro de un aggregate Example.
//   ExamplePublicId identifica el aggregate padre; ItemPublicId identifica el hijo buscado.
//   Contrato de respuesta: Result<GetExampleItemDto>, o Result.Failure(NotFound) si padre o hijo no existen.
public record GetExampleItemByPublicIdQuery(
    Guid ExamplePublicId,
    Guid ItemPublicId
) : IRequest<Result<GetExampleItemDto>>;
