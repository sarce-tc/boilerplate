using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Demonstrates the Dapper Unit-of-Work transaction pattern:
///
/// <list type="number">
///   <item>Open a shared Npgsql connection and BEGIN a transaction.</item>
///   <item>INSERT into <c>orders</c> → receive the auto-generated <c>id</c>.</item>
///   <item>INSERT each item into <c>order_items</c> using that <c>id</c> as FK.</item>
///   <item>COMMIT. Both inserts succeed or both roll back.</item>
/// </list>
///
/// All repository calls within a UoW share the same <see cref="Npgsql.NpgsqlConnection"/>
/// and <see cref="Npgsql.NpgsqlTransaction"/>, so every SQL statement participates
/// in the same ACID transaction without any additional configuration.
/// </summary>
public sealed class CreateOrderCommandHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        // ── 1. Open connection + BEGIN ────────────────────────────────────────
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ── 2. Insert Order header ────────────────────────────────────────
            var order = Order.Create(request.CustomerName);
            order = await unitOfWork.OrdersWrite.AddAsync(order, cancellationToken);
            // `order.Id` is now populated from RETURNING id

            // ── 3. Insert each OrderItem (same connection + transaction) ──────
            foreach (var dto in request.Items)
            {
                var item = OrderItem.CreateForOrder(
                    order.Id,
                    dto.ProductName,
                    dto.Quantity,
                    dto.UnitPrice);

                await unitOfWork.OrdersWrite.AddItemAsync(item, cancellationToken);
            }

            // ── 4. COMMIT — all rows land atomically ──────────────────────────
            await unitOfWork.CommitAsync(cancellationToken);

            return Result<Guid>.Success(order.PublicId);
        }
        catch
        {
            // ROLLBACK on any failure — no partial data persisted
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
