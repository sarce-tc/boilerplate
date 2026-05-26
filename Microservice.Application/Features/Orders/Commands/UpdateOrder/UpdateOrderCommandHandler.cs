using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Application.Features.Orders.Commands.UpdateOrder;

/// <summary>
/// Updates the customer name of a Pending order using a Dapper UoW transaction:
/// <list type="number">
///   <item>Read the order (no transaction needed).</item>
///   <item>Apply domain rule via <c>order.UpdateCustomerName()</c> — throws
///         <see cref="Microservice.Domain.Exceptions.DomainException"/> if not Pending,
///         handled by GlobalExceptionHandler → 409.</item>
///   <item>BEGIN → UPDATE orders SET customer_name = … → COMMIT.</item>
/// </list>
/// </summary>
public sealed class UpdateOrderCommandHandler(
    IOrderReadRepository orderReadRepo,
    IUnitOfWork          unitOfWork
) : IRequestHandler<UpdateOrderCommand, Result>
{
    public async Task<Result> Handle(
        UpdateOrderCommand request,
        CancellationToken  cancellationToken)
    {
        // ── 1. Read ───────────────────────────────────────────────────────────
        var order = await orderReadRepo.GetByPublicIdAsync(request.PublicId, cancellationToken);
        if (order is null)
            return Result.Failure(
                Error.NotFound($"Order '{request.PublicId}' was not found."));

        // ── 2. Apply domain rule ─────────────────────────────────────────────
        order.UpdateCustomerName(request.CustomerName);

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
