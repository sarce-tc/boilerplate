using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Orders.Commands.CancelOrder;

/// <summary>
/// Cancels an existing Order that has not yet been Completed.
/// Returns <see cref="Result.Failure"/> with a Conflict error if the order
/// is already in a terminal state.
/// </summary>
public sealed record CancelOrderCommand(Guid PublicId) : IRequest<Result>;
