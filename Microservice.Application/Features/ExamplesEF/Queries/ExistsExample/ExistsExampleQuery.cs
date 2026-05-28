using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Queries.ExistsExample;
// PATRÓN — Verifica si existe un aggregate Example con el PublicId dado sin cargar la entidad.
//   PublicId es el identificador público a verificar.
//   Contrato de respuesta: Result<bool> con true si existe, false si no.
public record ExistsExampleQuery(
    Guid PublicId
) : IRequest<Result<bool>>;
