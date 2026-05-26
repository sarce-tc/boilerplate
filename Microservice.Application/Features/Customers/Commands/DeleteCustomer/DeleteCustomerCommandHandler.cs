using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Application.Features.Customers.Commands.DeleteCustomer;

// Dapper write path: GetByPublicIdAsync + IUnitOfWork.CustomersWrite.DeleteAsync + CommitAsync
public sealed class DeleteCustomerCommandHandler(
    ICustomerReadRepository customerReadRepo,
    IUnitOfWork             unitOfWork
) : IRequestHandler<DeleteCustomerCommand, Result>
{
    public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken ct)
    {
        // 1. Read
        var customer = await customerReadRepo.GetByPublicIdAsync(request.PublicId, ct);
        if (customer is null)
            return Result.Failure(Error.NotFound($"Customer '{request.PublicId}' was not found."));

        // 2. Persist
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await unitOfWork.CustomersWrite.DeleteAsync(customer.Id, ct);
            await unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }

        return Result.Success();
    }
}
