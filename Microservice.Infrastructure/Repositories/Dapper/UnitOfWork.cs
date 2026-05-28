using Microservice.Application.Contracts.Persistence.Dapper;
using Npgsql;

namespace Microservice.Infrastructure.Repositories.Dapper;
// ═══════════════════════════════════════════════════════════════════════
// AGENT — UnitOfWork implementation
// Repos are lazy-initialized sharing the same NpgsqlConnection + NpgsqlTransaction.
// Never inject write repos directly via DI — always use IUnitOfWork.OrdersWrite.
//
// To add a new aggregate write repo:
//   1. Add: private MyEntityWriteRepository? _myEntityWrite;
//   2. Add: public IMyEntityWriteRepository MyEntityWrite =>
//               _myEntityWrite ??= new MyEntityWriteRepository(_connection!, _transaction!);
//   3. Add the property to IUnitOfWork interface.
//   4. Register IMyEntityWriteRepository in InfrastuctureServiceRegistration.cs.
// ═══════════════════════════════════════════════════════════════════════
public sealed class UnitOfWork(IDbConnectionFactory connectionFactory) : IUnitOfWork
{
    private NpgsqlConnection?  _connection;
    private NpgsqlTransaction? _transaction;

    private ExampleWriteRepository?  _exampleWrite;

    public IExampleWriteRepository ExamplesWrite =>
        _exampleWrite ??= new ExampleWriteRepository(_connection!, _transaction!);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _connection  = (NpgsqlConnection)await connectionFactory.CreateAsync(ct);
        _transaction = await _connection.BeginTransactionAsync(ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        try   { await _transaction!.CommitAsync(ct); }
        catch { await RollbackAsync(ct); throw; }
    }

    public async Task RollbackAsync(CancellationToken ct = default) =>
        await _transaction!.RollbackAsync(ct);

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null) await _transaction.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
    }
}
