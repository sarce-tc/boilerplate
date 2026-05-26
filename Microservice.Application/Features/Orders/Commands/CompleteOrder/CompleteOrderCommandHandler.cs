using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Application.Features.Orders.Commands.CompleteOrder;

/// <summary>
/// Marks an order as Completed using a Dapper UoW transaction:
/// <list type="number">
///   <item>Read the order (no transaction needed — read-only).</item>
///   <item>Guard: return NotFound if order does not exist.</item>
///   <item>Apply domain rule via <c>order.Complete()</c> — throws
///         <see cref="Microservice.Domain.Exceptions.DomainException"/> if Cancelled,
///         handled by GlobalExceptionHandler → 409.</item>
///   <item>BEGIN → UPDATE orders SET status = 'Completed' → COMMIT.</item>
/// </list>
/// </summary>
public sealed class CompleteOrderCommandHandler(
    IOrderReadRepository orderReadRepo,
    IUnitOfWork          unitOfWork
) : IRequestHandler<CompleteOrderCommand, Result>
{
    public async Task<Result> Handle(
        CompleteOrderCommand request,
        CancellationToken    cancellationToken)
    {
        // ── 1. Read ───────────────────────────────────────────────────────────
        var order = await orderReadRepo.GetByPublicIdAsync(request.PublicId, cancellationToken);
        if (order is null)
            return Result.Failure(
                Error.NotFound($"Order '{request.PublicId}' was not found."));

        // ── 2. Apply domain rule ─────────────────────────────────────────────
        // DomainException propagates to GlobalExceptionHandler → 409 Conflict.
        // Called before BeginTransactionAsync — no rollback needed on failure.
        order.Complete();

        // ── 3. Persist within a transaction ───────────────────────────────────
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
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
