using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Application.Features.Orders.Commands.RemoveOrderItem;

/// <summary>
/// Removes a line item from a Pending order using a Dapper UoW transaction:
/// <list type="number">
///   <item>Read order + all items (single JOIN query via <c>GetWithItemsAsync</c>).</item>
///   <item>Apply domain rule via <c>order.RemoveItemForDapper()</c>:
///         validates Pending status, finds the item, subtracts its LineTotal.</item>
///   <item>BEGIN → DELETE order_item → UPDATE orders total_amount → COMMIT.</item>
/// </list>
/// </summary>
public sealed class RemoveOrderItemCommandHandler(
    IOrderReadRepository orderReadRepo,
    IUnitOfWork          unitOfWork
) : IRequestHandler<RemoveOrderItemCommand, Result>
{
    public async Task<Result> Handle(
        RemoveOrderItemCommand request,
        CancellationToken      cancellationToken)
    {
        // ── 1. Read order + items (needed to locate the item and recalculate total) ─
        var (order, items) = await orderReadRepo.GetWithItemsAsync(request.OrderPublicId, cancellationToken);
        if (order is null)
            return Result.Failure(
                Error.NotFound($"Order '{request.OrderPublicId}' was not found."));

        // ── 2. Domain rule: EnsureModifiable + locate item + subtract total ────
        // DomainException → GlobalExceptionHandler → 409. No TX open yet.
        var item = order.RemoveItemForDapper(request.ItemPublicId, items);

        // ── 3. Persist within a transaction ───────────────────────────────────
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await unitOfWork.OrdersWrite.RemoveItemAsync(item.Id, cancellationToken);
            await unitOfWork.OrdersWrite.UpdateAsync(order, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        return Result.Success();
    }
}
