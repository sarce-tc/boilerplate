using MediatR;
using Microservice.Application.Common;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Customers;

namespace Microservice.Application.Features.Customers.Queries.GetCustomers;

public sealed class GetCustomersQueryHandler(
    ICustomerReadRepository customerReadRepo
) : IRequestHandler<GetCustomersQuery, Result<PagedResult<CustomerSummaryDto>>>
{
    public async Task<Result<PagedResult<CustomerSummaryDto>>> Handle(
        GetCustomersQuery request, CancellationToken ct)
    {
        var (customers, total) = await customerReadRepo.GetPagedAsync(request.Page, request.PageSize, ct);
        return Result<PagedResult<CustomerSummaryDto>>.Success(
            new PagedResult<CustomerSummaryDto>(customers, total, request.Page, request.PageSize));
    }
}
