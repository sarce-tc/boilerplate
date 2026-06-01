using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.CustomersEF.Queries.GetCustomerById;
// PATRÓN — Detalle de un Customer por PublicId.
public record GetCustomerByIdQuery(Guid PublicId) : IRequest<Result<GetCustomerDto>>;
