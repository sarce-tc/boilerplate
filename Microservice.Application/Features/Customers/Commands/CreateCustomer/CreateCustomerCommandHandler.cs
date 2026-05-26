using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Customers.Commands.CreateCustomer;

// Dapper write path: Customer.Create (domain factory) + IUnitOfWork.CustomersWrite + CommitAsync
public sealed class CreateCustomerCommandHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        // 1. Domain factory — throws ArgumentException → GlobalExceptionHandler → 400
        var customer = Customer.Create(request.Name, request.Email, request.Phone);

        // 2. Persist
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var created = await unitOfWork.CustomersWrite.AddAsync(customer, ct);
            await unitOfWork.CommitAsync(ct);
            return Result<Guid>.Success(created.PublicId);
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }
    }
}
