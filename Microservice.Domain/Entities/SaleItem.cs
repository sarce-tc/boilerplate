using Microservice.Domain.Exceptions;
using Microservice.Domain.ValueObjects;

namespace Microservice.Domain.Entities;

/// <summary>
/// Entidad hija del aggregate <see cref="Sale"/>: una línea de venta con snapshot del producto.
/// <para>
/// Guarda nombre/precio/alícuota al momento de la venta (no referencia viva al producto) para que
/// la venta sea inmutable ante cambios posteriores del catálogo. Se crea vía <see cref="Sale.AddItem"/>.
/// </para>
/// </summary>
public sealed class SaleItem : BaseDomainModel
{
    // ── Constraints ──────────────────────────────────────────────────────────
    public const int ProductNameMaxLength = 200;

    /// <summary>FK a la <see cref="Sale"/> propietaria. La fija EF por relationship fixup.</summary>
    public int SaleId { get; private set; }

    /// <summary>PublicId del <see cref="Product"/> vendido (para descontar stock).</summary>
    public Guid ProductPublicId { get; private set; }

    /// <summary>Nombre del producto al momento de la venta (snapshot).</summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>Cantidad vendida (soporta fracciones).</summary>
    public decimal Quantity { get; private set; }

    /// <summary>Precio unitario neto (sin IVA) al momento de la venta.</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>Alícuota de IVA aplicada (porcentaje).</summary>
    public decimal TaxRate { get; private set; }

    /// <summary>Neto de la línea (UnitPrice × Quantity).</summary>
    public decimal LineNet { get; private set; }

    /// <summary>IVA de la línea (LineNet × TaxRate / 100).</summary>
    public decimal LineTax { get; private set; }

    /// <summary>Total de la línea (LineNet + LineTax).</summary>
    public decimal LineTotal { get; private set; }

    // ── EF Core parameterless constructor (infrastructure only) ──────────────
    private SaleItem() { }

    // ── Package-internal factory (llamado solo por Sale.AddItem) ─────────────
    internal SaleItem(Guid productPublicId, string productName, decimal quantity, decimal unitPrice, decimal taxRate)
    {
        if (productPublicId == Guid.Empty)
            throw new ArgumentException("ProductPublicId is required.", nameof(productPublicId));
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (unitPrice < 0)
            throw new DomainException("UnitPrice must not be negative.");
        if (taxRate is < 0 or > 100)
            throw new DomainException("TaxRate must be between 0 and 100.");

        ProductPublicId = productPublicId;
        ProductName     = productName.Trim();
        Quantity        = quantity;
        UnitPrice       = unitPrice;
        TaxRate         = taxRate;

        LineNet   = unitPrice * quantity;
        LineTax   = LineNet * taxRate / 100m;
        LineTotal = LineNet + LineTax;

        PublicId  = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
