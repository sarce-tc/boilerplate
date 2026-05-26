using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Application.Features.Orders.Commands.AddOrderItem;

/// <summary>
/// Adds a line item to an existing Pending order using a Dapper UoW transaction:
/// <list type="number">
///   <item>Read the order header (no items needed for this operation).</item>
///   <item>Apply domain rule via <c>order.AddItemForDapper()</c>:
///         validates Pending status, creates the item, updates TotalAmount in memory.</item>
///   <item>BEGIN → INSERT order_item → UPDATE orders total_amount → COMMIT.</item>
/// </list>
/// Returns the new item's <see cref="Guid">PublicId</see>.
/// </summary>
public sealed class AddOrderItemCommandHandler(
    IOrderReadRepository orderReadRepo,
    IUnitOfWork          unitOfWork
) : IRequestHandler<AddOrderItemCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddOrderItemCommand request,
        CancellationToken   cancellationToken)
    {
        // ── 1. Read order header ──────────────────────────────────────────────
        var order = await orderReadRepo.GetByPublicIdAsync(request.OrderPublicId, cancellationToken);
        if (order is null)
            return Result<Guid>.Failure(
                Error.NotFound($"Order '{request.OrderPublicId}' was not found."));

        // ── 2. Domain rule: EnsureModifiable + create item + update total ─────
        // DomainException → GlobalExceptionHandler → 409. No TX open yet.
        var newItem = order.AddItemForDapper(request.ProductName, request.Quantity, request.UnitPrice);

        // ── 3. Persist within a transaction ───────────────────────────────────
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // INSERT order_item → RETURNING assigns the DB id to newItem
            newItem = await unitOfWork.OrdersWrite.AddItemAsync(newItem, cancellationToken);
            // UPDATE orders to reflect the new TotalAmount
            await unitOfWork.OrdersWrite.UpdateAsync(order, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        return Result<Guid>.Success(newItem.PublicId);
    }
}
