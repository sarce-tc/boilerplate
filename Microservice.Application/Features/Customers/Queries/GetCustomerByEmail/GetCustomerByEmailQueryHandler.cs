using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Customers;

namespace Microservice.Application.Features.Customers.Queries.GetCustomerByEmail;

public sealed class GetCustomerByEmailQueryHandler(
    ICustomerReadRepository customerReadRepo
) : IRequestHandler<GetCustomerByEmailQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByEmailQuery request, CancellationToken ct)
    {
        var customer = await customerReadRepo.GetByEmailAsync(request.Email, ct);
        if (customer is null)
            return Result<CustomerDto>.Failure(
                Error.NotFound($"Customer with email '{request.Email}' was not found."));

        return Result<CustomerDto>.Success(new CustomerDto(
            customer.PublicId,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.CreatedAt));
    }
}
