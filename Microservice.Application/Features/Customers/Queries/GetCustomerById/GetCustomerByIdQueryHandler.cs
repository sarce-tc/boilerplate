using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Customers;

namespace Microservice.Application.Features.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler(
    ICustomerReadRepository customerReadRepo
) : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        var customer = await customerReadRepo.GetByPublicIdAsync(request.PublicId, ct);
        if (customer is null)
            return Result<CustomerDto>.Failure(
                Error.NotFound($"Customer '{request.PublicId}' was not found."));

        return Result<CustomerDto>.Success(new CustomerDto(
            customer.PublicId,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.CreatedAt));
    }
}
