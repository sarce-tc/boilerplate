using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.CustomersEF.Commands.DeleteCustomer;
// PATRÓN — Elimina un Customer por su PublicId.
public record DeleteCustomerCommand(Guid PublicId) : IRequest<Result<Guid>>;
