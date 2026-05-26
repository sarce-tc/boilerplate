using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Application.Features.Customers.Commands.UpdateCustomer;

// Dapper write path: GetByPublicIdAsync + customer.Update (domain) + IUnitOfWork + CommitAsync
public sealed class UpdateCustomerCommandHandler(
    ICustomerReadRepository customerReadRepo,
    IUnitOfWork             unitOfWork
) : IRequestHandler<UpdateCustomerCommand, Result>
{
    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        // 1. Read
        var customer = await customerReadRepo.GetByPublicIdAsync(request.PublicId, ct);
        if (customer is null)
            return Result.Failure(Error.NotFound($"Customer '{request.PublicId}' was not found."));

        // 2. Domain — throws ArgumentException if Name/Email are blank → 400
        customer.Update(request.Name, request.Email, request.Phone);

        // 3. Persist
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await unitOfWork.CustomersWrite.UpdateAsync(customer, ct);
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
