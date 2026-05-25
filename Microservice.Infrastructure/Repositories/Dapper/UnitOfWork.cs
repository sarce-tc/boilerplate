using Microservice.Application.Contracts.Persistence.Dapper;
using Npgsql;

namespace Microservice.Infrastructure.Repositories.Dapper
{
    public sealed class UnitOfWork(IDbConnectionFactory connectionFactory) : Application.Contracts.Persistence.Dapper.IUnitOfWork
    {
        private NpgsqlConnection? _connection;
        private NpgsqlTransaction? _transaction;


        private ExampleWriteRepository? _exampleWrite;
        private ProductWriteRepository? _productWrite;
        private OrderWriteRepository?   _orderWrite;

        public IExampleWriteRepository ExamplesWrite =>
            _exampleWrite ??= new ExampleWriteRepository(_connection!, _transaction!);

        public IProductWriteRepository ProductWrite =>
            _productWrite ??= new ProductWriteRepository(_connection!, _transaction!);

        public IOrderWriteRepository OrdersWrite =>
            _orderWrite ??= new OrderWriteRepository(_connection!, _transaction!);

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            _connection = (NpgsqlConnection)await connectionFactory.CreateAsync(ct);
            _transaction = await _connection.BeginTransactionAsync(ct);
        }

        public async Task CommitAsync(CancellationToken ct = default)
        {
            try
            {
                await _transaction!.CommitAsync(ct);  // ← ahora compila
            }
            catch
            {
                await RollbackAsync(ct);
                throw;
            }
        }

        public async Task RollbackAsync(CancellationToken ct = default) =>
            await _transaction!.RollbackAsync(ct);  // ← ahora compila

        public async ValueTask DisposeAsync()
        {
            if (_transaction is not null) await _transaction.DisposeAsync();
            if (_connection is not null) await _connection.DisposeAsync();
        }
    }
}
