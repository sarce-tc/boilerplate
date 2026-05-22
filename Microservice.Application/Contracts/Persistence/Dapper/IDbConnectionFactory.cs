using System.Data;

namespace Microservice.Application.Contracts.Persistence.Dapper
{
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> CreateAsync(CancellationToken ct = default);
    }
}
