using Dapper;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.Common;
using Npgsql;
using System.Data;

namespace Microservice.Infrastructure.Repositories.Dapper
{
    // ═══════════════════════════════════════════════════════════════════════
    // AGENT — Dapper read base. Subclass and override TableName (snake_case).
    // Generic queries provided: GetByIdAsync · GetByPublicIdAsync · GetAllAsync
    //                           ExistsAsync · CountAsync
    // Add specific queries in the subclass only when the generic ones don't fit.
    // DefaultTypeMap.MatchNamesWithUnderscores = true → customer_name → CustomerName
    // ═══════════════════════════════════════════════════════════════════════
    public abstract class ReadRepository<T>
        : IReadRepository<T> where T : BaseDomainModel
    {
        protected abstract string TableName { get; }
        protected virtual string SelectColumns => "*";

        private readonly IDbConnectionFactory? _connectionFactory;
        protected readonly IDbConnection? _externalConnection;
        protected readonly IDbTransaction? _transaction;

        protected ReadRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        protected ReadRepository(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            _externalConnection = connection;
            _transaction = transaction;
        }


        protected async Task<IDbConnection> GetConnectionAsync(CancellationToken ct)
        {
            if (_externalConnection is not null) return _externalConnection;
            return await _connectionFactory!.CreateAsync(ct);
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            using var conn = await GetConnectionAsync(ct);
            var sql = $"SELECT {SelectColumns} FROM {TableName} WHERE id = @Id";
            return await conn.QuerySingleOrDefaultAsync<T>(sql, new { Id = id });
        }

        public async Task<T?> GetByPublicIdAsync(Guid publicId, CancellationToken ct = default)
        {
            using var conn = await GetConnectionAsync(ct);
            var sql = $"SELECT {SelectColumns} FROM {TableName} WHERE public_id = @PublicId";
            return await conn.QuerySingleOrDefaultAsync<T>(sql, new { PublicId = publicId });
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        {
            using var conn = await GetConnectionAsync(ct);
            var sql = $"SELECT {SelectColumns} FROM {TableName}";
            var result = await conn.QueryAsync<T>(sql);
            return result.ToList().AsReadOnly();
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        {
            using var conn = await GetConnectionAsync(ct);
            var sql = $"SELECT EXISTS(SELECT 1 FROM {TableName} WHERE id = @Id)";
            return await conn.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<int> CountAsync(CancellationToken ct = default)
        {
            using var conn = await GetConnectionAsync(ct);
            return await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {TableName}");
        }
    }
}
