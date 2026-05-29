using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleItems;
// PATRÓN — Obtiene la colección de hijos Items de un aggregate Example identificado por su PublicId.
//   ExamplePublicId identifica el aggregate padre cuyos Items se quieren listar.
//   Contrato de respuesta: Result<IEnumerable<GetExampleItemDto>>, o Result.Failure(NotFound) si el padre no existe.
public record GetExampleItemsQuery(Guid ExamplePublicId) : IRequest<Result<IEnumerable<GetExampleItemDto>>>;
