// ═══════════════════════════════════════════════════════════════════════════
// AGENT ENTRY POINT — Dapper write path (all mutations that need atomicity)
//
// Usage pattern in every command handler:
//   1. Read  → IOrderReadRepository (no TX, read-only)
//   2. Apply → order.DomainMethod() (DomainException → 409, no TX open yet)
//   3. Write → BeginTransactionAsync → try { repos } catch { Rollback; throw }
//
// Write repositories available:
//   ExamplesWrite  → AddAsync / UpdateAsync / DeleteAsync
//   OrdersWrite    → AddAsync / UpdateAsync / AddItemAsync / RemoveItemAsync
//   CustomersWrite → AddAsync / UpdateAsync / DeleteAsync
//
// Implementation: Infrastructure/Repositories/Dapper/UnitOfWork.cs
// ═══════════════════════════════════════════════════════════════════════════
namespace Microservice.Application.Contracts.Persistence.Dapper
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IExampleWriteRepository  ExamplesWrite  { get; }
        Task BeginTransactionAsync(CancellationToken ct = default);
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
    }
}
