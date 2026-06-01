using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Interfaces;

/// <summary>
/// Domain service que orquesta la confirmación de una <see cref="Sale"/> a través de varios aggregates:
/// descuenta stock (vía <see cref="IInventoryDomainService"/>), registra el cobro en la caja
/// (<see cref="CashSession"/>) y transiciona la venta a confirmada.
/// <para>
/// Lógica de dominio pura (sin I/O): el caller precarga los aggregates (Sale, CashSession y los
/// StockItem de cada producto) y persiste el resultado en una sola TX vía <c>IUnitOfWork</c>.
/// Los asientos de inventario creados se devuelven para que el caller los agregue.
/// </para>
/// </summary>
public interface ISaleDomainService
{
    /// <summary>
    /// Confirma la venta: por cada ítem descuenta el stock del producto y genera su asiento de
    /// inventario; registra un movimiento de caja por el total; marca la venta como confirmada.
    /// </summary>
    /// <param name="sale">Venta pendiente (con ítems), tracked.</param>
    /// <param name="cashSession">Turno de caja abierto donde se cobra, tracked.</param>
    /// <param name="stockByProduct">Saldos de stock por <c>ProductPublicId</c> (tracked) de los productos vendidos.</param>
    /// <returns>Los asientos de inventario generados (a persistir por el caller).</returns>
    /// <exception cref="Exceptions.DomainException">
    /// Venta no pendiente, caja cerrada, falta de registro de stock o stock insuficiente.
    /// </exception>
    IReadOnlyList<InventoryMovement> Confirm(
        Sale sale,
        CashSession cashSession,
        IReadOnlyDictionary<Guid, StockItem> stockByProduct);
}
