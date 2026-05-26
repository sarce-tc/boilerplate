using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Orders.Commands.RemoveOrderItem;

/// <param name="OrderPublicId">The parent order.</param>
/// <param name="ItemPublicId">The item to remove.</param>
public record RemoveOrderItemCommand(Guid OrderPublicId, Guid ItemPublicId) : IRequest<Result>;
