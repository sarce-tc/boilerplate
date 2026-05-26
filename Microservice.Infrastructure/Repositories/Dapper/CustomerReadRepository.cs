using Dapper;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Customers;
using Microservice.Domain.Entities;

namespace Microservice.Infrastructure.Repositories.Dapper;

public sealed class CustomerReadRepository : ReadRepository<Customer>, ICustomerReadRepository
{
    protected override string TableName => "customers";

    public CustomerReadRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory) { }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);
        return await conn.QuerySingleOrDefaultAsync<Customer>(
            "SELECT * FROM customers WHERE email = @Email",
            new { Email = email });
    }

    public async Task<(IReadOnlyList<CustomerSummaryDto> Customers, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = """
            SELECT public_id, name, email, created_at
            FROM customers
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset;

            SELECT COUNT(*)::int FROM customers;
            """;

        using var multi = await conn.QueryMultipleAsync(
            sql,
            new { PageSize = pageSize, Offset = (page - 1) * pageSize });

        var customers = (await multi.ReadAsync<CustomerSummaryDto>()).ToList().AsReadOnly();
        var total     = await multi.ReadSingleAsync<int>();

        return (customers, total);
    }
}
