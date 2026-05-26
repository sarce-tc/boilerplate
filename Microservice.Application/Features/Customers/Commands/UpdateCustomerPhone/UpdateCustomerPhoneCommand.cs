using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Customers.Commands.UpdateCustomerPhone;

public record UpdateCustomerPhoneCommand(Guid PublicId, string? Phone) : IRequest<Result>;
