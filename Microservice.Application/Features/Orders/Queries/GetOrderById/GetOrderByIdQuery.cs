using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.Orders;

namespace Microservice.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Returns a full <see cref="OrderDto"/> (header + items) for the given PublicId.
/// Executed via a single Dapper JOIN query — no EF context involved.
/// </summary>
public sealed record GetOrderByIdQuery(Guid PublicId) : IRequest<Result<OrderDto>>;
