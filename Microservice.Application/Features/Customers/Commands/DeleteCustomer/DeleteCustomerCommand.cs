using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(Guid PublicId) : IRequest<Result>;
