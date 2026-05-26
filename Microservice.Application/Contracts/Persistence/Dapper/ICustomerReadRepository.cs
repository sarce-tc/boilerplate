using Microservice.Application.DTOs.Customers;
using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.Dapper;

public interface ICustomerReadRepository : IReadRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<(IReadOnlyList<CustomerSummaryDto> Customers, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default);
}
