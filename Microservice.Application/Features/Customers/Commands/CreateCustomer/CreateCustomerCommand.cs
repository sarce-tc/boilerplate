using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string  Name,
    string  Email,
    string? Phone = null) : IRequest<Result<Guid>>;
