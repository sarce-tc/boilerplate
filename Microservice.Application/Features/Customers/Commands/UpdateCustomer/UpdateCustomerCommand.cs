using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(
    Guid    PublicId,
    string  Name,
    string  Email,
    string? Phone = null) : IRequest<Result>;
