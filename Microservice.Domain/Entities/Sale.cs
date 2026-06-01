using Microservice.Domain.Exceptions;
using Microservice.Domain.ValueObjects;

namespace Microservice.Domain.Entities;

/// <summary>
/// Aggregate root de una venta del POS (parent-child con <see cref="SaleItem"/>).
/// <para>
/// El aggregate gobierna sus ítems, totales y transiciones de estado. Los efectos cross-aggregate
/// de la confirmación (descontar stock, registrar el cobro en caja, facturar) los coordina
/// <c>ISaleDomainService</c>; el aggregate solo transiciona su propio estado vía <see cref="Confirm"/>.
/// </para>
/// </summary>
public sealed class Sale : BaseDomainModel
{
    // ── Properties ───────────────────────────────────────────────────────────
    /// <summary>Cliente de la venta (opcional: consumidor final sin identificar).</summary>
    public Guid? CustomerPublicId { get; private set; }

    /// <summary>Turno de caja donde se registra el cobro. Requerido.</summary>
    public Guid CashSessionPublicId { get; private set; }

    /// <summary>Estado de la venta.</summary>
    public SaleStatus Status { get; private set; } = SaleStatus.Pending;

    /// <summary>Neto total (Σ líneas sin IVA).</summary>
    public decimal Subtotal { get; private set; }

    /// <summary>IVA total (Σ IVA de líneas).</summary>
    public decimal TaxAmount { get; private set; }

    /// <summary>Total a cobrar (Subtotal + IVA).</summary>
    public decimal Total { get; private set; }

    /// <summary>Momento de confirmación.</summary>
    public DateTimeOffset? ConfirmedAt { get; private set; }

    /// <summary>Comprobante electrónico asociado (gancho para facturación AFIP — vertical 6).</summary>
    public Guid? InvoicePublicId { get; private set; }

    // ── Items (encapsulados — mutados solo por métodos de dominio) ────────────
    private readonly List<SaleItem> _items = [];

    /// <summary>Líneas de la venta. Solo lectura fuera del aggregate.</summary>
    public IReadOnlyList<SaleItem> Items => _items.AsReadOnly();

    // ── EF Core parameterless constructor (infrastructure only) ──────────────
    private Sale() { _items = []; }

    // ── Factory constructor ──────────────────────────────────────────────────
    /// <exception cref="ArgumentException">CashSessionPublicId vacío.</exception>
    public Sale(Guid? customerPublicId, Guid cashSessionPublicId)
    {
        if (cashSessionPublicId == Guid.Empty)
            throw new ArgumentException("CashSessionPublicId is required.", nameof(cashSessionPublicId));

        CustomerPublicId    = customerPublicId;
        CashSessionPublicId = cashSessionPublicId;
        Status              = SaleStatus.Pending;
        PublicId            = Guid.NewGuid();
        CreatedAt           = DateTimeOffset.UtcNow;
        UpdatedAt           = DateTimeOffset.UtcNow;
    }

    // ── Item management ────────────────────────────────────────────────────────

    /// <summary>Agrega una línea con snapshot del producto.</summary>
    /// <exception cref="DomainException">Venta no pendiente.</exception>
    public SaleItem AddItem(Guid productPublicId, string productName, decimal quantity, decimal unitPrice, decimal taxRate)
    {
        EnsurePending();

        var item = new SaleItem(productPublicId, productName, quantity, unitPrice, taxRate);
        _items.Add(item);
        Recalculate();
        return item;
    }

    /// <exception cref="DomainException">Venta no pendiente o ítem inexistente.</exception>
    public void RemoveItem(Guid itemPublicId)
    {
        EnsurePending();

        var item = _items.Find(i => i.PublicId == itemPublicId)
            ?? throw new DomainException($"Item '{itemPublicId}' not found in this sale.");

        _items.Remove(item);
        Recalculate();
    }

    // ── State transitions ───────────────────────────────────────────────────────

    /// <summary>
    /// Marca la venta como confirmada. Los efectos cross-aggregate (stock, caja, factura)
    /// los aplica el domain service ANTES/junto a esta transición.
    /// </summary>
    /// <exception cref="DomainException">Venta no pendiente o sin ítems.</exception>
    public void Confirm()
    {
        EnsurePending();
        if (_items.Count == 0)
            throw new DomainException("Cannot confirm a sale with no items.");

        Status      = SaleStatus.Confirmed;
        ConfirmedAt = DateTimeOffset.UtcNow;
        UpdatedAt   = DateTimeOffset.UtcNow;
    }

    /// <exception cref="DomainException">Venta no pendiente (las confirmadas requieren flujo de devolución).</exception>
    public void Cancel()
    {
        EnsurePending();

        Status    = SaleStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Asocia el comprobante electrónico emitido (vertical 6 — facturación).</summary>
    /// <exception cref="DomainException">Venta no confirmada o factura ya asociada.</exception>
    public void AttachInvoice(Guid invoicePublicId)
    {
        if (Status != SaleStatus.Confirmed)
            throw new DomainException("Only a confirmed sale can be invoiced.");
        if (InvoicePublicId is not null)
            throw new DomainException("This sale already has an invoice attached.");

        InvoicePublicId = invoicePublicId;
        UpdatedAt       = DateTimeOffset.UtcNow;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private void Recalculate()
    {
        Subtotal  = _items.Sum(i => i.LineNet);
        TaxAmount = _items.Sum(i => i.LineTax);
        Total     = _items.Sum(i => i.LineTotal);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void EnsurePending()
    {
        if (Status != SaleStatus.Pending)
            throw new DomainException($"Cannot modify a sale in '{Status}' state.");
    }
}
