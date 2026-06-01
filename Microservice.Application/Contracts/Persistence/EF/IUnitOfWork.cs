using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.EF;
// Contrato del Unit of Work EF Core — agrupa los repositorios de escritura del bounded context
// y expone SaveChangesAsync para confirmar todos los cambios staged en una TX implícita de PostgreSQL.
// ExamplesWrite: repositorio específico del aggregate Example; usar cuando se necesitan métodos
//   que no existen en la superficie genérica de WriteRepository.
// WriteRepository: superficie genérica de escritura; punto de entrada por defecto para bulk ops
//   (DeleteManyAsync, UpdateManyAsync) y writes que no requieren métodos específicos del aggregate.
// SaveChangesAsync: confirma todos los cambios pendientes del DbContext en una única TX implícita.
public interface IUnitOfWork
{
    IExampleWriteRepository   ExamplesWrite   { get; }
    IWriteRepository<Example> WriteRepository { get; }

    // POS aggregates — generic-first: superficie genérica de LINQRepository<T>.
    // No requieren contrato específico (sin ILike/JOIN/SQL crudo en escritura).
    IWriteRepository<Product>           ProductsWrite           { get; }
    IWriteRepository<Customer>          CustomersWrite          { get; }
    IWriteRepository<StockItem>         StockItemsWrite         { get; }
    IWriteRepository<InventoryMovement> InventoryMovementsWrite { get; }
    IWriteRepository<CashSession>       CashSessionsWrite       { get; }
    IWriteRepository<Sale>              SalesWrite              { get; }
    IWriteRepository<Invoice>           InvoicesWrite           { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
