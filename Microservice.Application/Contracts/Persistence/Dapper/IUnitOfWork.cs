namespace Microservice.Application.Contracts.Persistence.Dapper
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IExampleWriteRepository ExamplesWrite { get; }
        IProductWriteRepository ProductWrite { get; }

        Task BeginTransactionAsync(CancellationToken ct = default);
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
    }
}
