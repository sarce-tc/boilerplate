using Dapper;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Domain.Common;
using Npgsql;
using System.Data;

namespace Microservice.Infrastructure.Repositories.Dapper
{
    // Modificar WriteRepository<T> para aceptar conexión externa
    public abstract class WriteRepository<T> : IWriteRepository<T>
        where T : BaseDomainModel
    {
        private readonly IDbConnectionFactory? _connectionFactory;
        protected readonly IDbConnection? _externalConnection;
        protected IDbTransaction? _transaction;

        // Constructor normal — crea su propia conexión
        protected WriteRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // Constructor para UnitOfWork — usa conexión compartida
        protected WriteRepository(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            _externalConnection = connection;
            _transaction = transaction;
        }

        protected async Task<IDbConnection> GetConnectionAsync(CancellationToken ct)
        {
            if (_externalConnection is not null) return _externalConnection;
            return await _connectionFactory!.CreateAsync(ct);
        }

        public abstract Task<T> AddAsync(T entity, CancellationToken ct = default);
        public abstract Task<T> UpdateAsync(T entity, CancellationToken ct = default);

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var conn = await GetConnectionAsync(ct);
            await conn.ExecuteAsync(
                $"DELETE FROM {TableName} WHERE id = @Id",
                new { Id = id },
                _transaction);  // ← pasa la transacción si existe
        }

        protected abstract string TableName { get; }
    }
}
