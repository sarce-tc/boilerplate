using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleWithItems;
// PATRÓN — Obtiene un aggregate Example junto con su colección de hijos Items en una sola consulta.
//   PublicId identifica el aggregate raíz; Items se carga mediante eager-loading via includeProperties.
//   Contrato de respuesta: Result<GetExampleWithItemsDto>, o Result.Failure(NotFound) si no existe.
public record GetExampleWithItemsQuery(Guid PublicId) : IRequest<Result<GetExampleWithItemsDto>>;
