using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Application.Features.Orders.Commands.CancelOrder;

/// <summary>
/// Cancels an order using a Dapper UoW transaction:
/// <list type="number">
///   <item>Read the order (no transaction needed — read-only).</item>
///   <item>Guard: return Conflict if already in a terminal state.</item>
///   <item>Apply domain rule via <c>order.Cancel()</c>.</item>
///   <item>BEGIN → UPDATE orders SET status = 'Cancelled' → COMMIT.</item>
/// </list>
/// </summary>
// REFERENCE IMPLEMENTATION — plantilla para command handlers con lógica de dominio.
public sealed class CancelOrderCommandHandler(
    IOrderReadRepository orderReadRepo,
    IUnitOfWork          unitOfWork
) : IRequestHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(
        CancelOrderCommand request,
        CancellationToken  cancellationToken)
    {
        // ── 1. Read ───────────────────────────────────────────────────────────
        var order = await orderReadRepo.GetByPublicIdAsync(request.PublicId, cancellationToken);
        if (order is null)
            return Result.Failure(
                Error.NotFound($"Order '{request.PublicId}' was not found."));

        // ── 2. Apply domain rule ─────────────────────────────────────────────
        // DomainException propagates to GlobalExceptionHandler → 409 Conflict.
        // Called before BeginTransactionAsync, so no rollback is needed on failure.
        order.Cancel();

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
