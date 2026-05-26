using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Application.Features.Customers.Commands.UpdateCustomerPhone;

public sealed class UpdateCustomerPhoneCommandHandler(
    ICustomerReadRepository customerReadRepo,
    IUnitOfWork             unitOfWork
) : IRequestHandler<UpdateCustomerPhoneCommand, Result>
{
    public async Task<Result> Handle(UpdateCustomerPhoneCommand request, CancellationToken ct)
    {
        // 1. Leer
        var customer = await customerReadRepo.GetByPublicIdAsync(request.PublicId, ct);
        if (customer is null)
            return Result.Failure(Error.NotFound($"Customer '{request.PublicId}' was not found."));

        // 2. Dominio ANTES de BeginTransactionAsync
        customer.Update(customer.Name, customer.Email, request.Phone);

        // 3. Persistir
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
