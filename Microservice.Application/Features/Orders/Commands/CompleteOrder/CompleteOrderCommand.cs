using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Orders.Commands.CompleteOrder;

public record CompleteOrderCommand(Guid PublicId) : IRequest<Result>;
