using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Orders.Commands.UpdateOrder;

/// <param name="PublicId">Order to update.</param>
/// <param name="CustomerName">New customer name (non-empty, max 200 chars).</param>
public record UpdateOrderCommand(Guid PublicId, string CustomerName) : IRequest<Result>;
