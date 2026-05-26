using Dapper;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.Entities;
using Npgsql;
using System.Data;

namespace Microservice.Infrastructure.Repositories.Dapper
{
    public class ExampleWriteRepository : WriteRepository<Example>, IExampleWriteRepository
    {
        protected override string TableName => "examples";

        public ExampleWriteRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory) { }

        public ExampleWriteRepository(NpgsqlConnection connection, NpgsqlTransaction transaction)
            : base(connection, transaction) { }

        public override async Task<Example> AddAsync(Example entity, CancellationToken ct = default)
        {
            var conn = await GetConnectionAsync(ct);
            const string sql = """
            INSERT INTO examples (public_id, name, description)
            VALUES (@PublicId, @Name, @Description)
            RETURNING id, public_id, name, description, created_at, updated_at
            """;
            return await conn.QuerySingleAsync<Example>(sql, entity, _transaction);
        }

        public override async Task<Example> UpdateAsync(Example entity, CancellationToken ct = default)
        {
            var conn = await GetConnectionAsync(ct);
            const string sql = """
            UPDATE examples
            SET name        = @Name,
                description = @Description,
                updated_at  = NOW()
            WHERE id = @Id
            RETURNING id, public_id, name, description, created_at, updated_at
            """;
            return await conn.QuerySingleAsync<Example>(sql, entity, _transaction);
        }
    }
}
