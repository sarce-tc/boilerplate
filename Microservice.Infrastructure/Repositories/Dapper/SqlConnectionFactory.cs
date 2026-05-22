using Microservice.Application.Contracts.Persistence.Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace Microservice.Infrastructure.Repositories.Dapper
{

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not configured");
    }

    public async Task<IDbConnection> CreateAsync(CancellationToken ct = default)
    {
        var connection = new NpgsqlConnection(_connectionString);

        await connection.OpenAsync(ct);

        return connection;
    }
}

}