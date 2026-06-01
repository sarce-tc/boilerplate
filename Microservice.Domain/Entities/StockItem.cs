using Microservice.Domain.Exceptions;
using Microservice.Domain.ValueObjects;

namespace Microservice.Domain.Entities;

/// <summary>
/// Aggregate root del saldo materializado de existencias de un producto.
/// <para>
/// Referencia al producto por <see cref="ProductPublicId"/> (id de otro aggregate, sin navegación EF
/// para respetar los límites del aggregate). El saldo se ajusta exclusivamente con
/// <see cref="Increase"/>/<see cref="Decrease"/>, normalmente desde el domain service de inventario,
/// que además deja el rastro en <see cref="InventoryMovement"/> (ledger).
/// </para>
/// </summary>
public sealed class StockItem : BaseDomainModel
{
    /// <summary>PublicId del <see cref="Product"/> al que pertenece el saldo. Único.</summary>
    public Guid ProductPublicId { get; private set; }

    /// <summary>Cantidad disponible. Soporta fracciones (productos por peso). Nunca negativa.</summary>
    public decimal QuantityOnHand { get; private set; }

    // ── EF Core parameterless constructor (infrastructure only) ──────────────
    private StockItem() { }

    /// <exception cref="ArgumentException">ProductPublicId vacío.</exception>
    /// <exception cref="DomainException">Cantidad inicial negativa.</exception>
    public StockItem(Guid productPublicId, decimal initialQuantity = 0m)
    {
        if (productPublicId == Guid.Empty)
            throw new ArgumentException("ProductPublicId is required.", nameof(productPublicId));
        if (initialQuantity < 0)
            throw new DomainException("Initial quantity must not be negative.");

        ProductPublicId = productPublicId;
        QuantityOnHand  = initialQuantity;
        PublicId        = Guid.NewGuid();
        CreatedAt       = DateTimeOffset.UtcNow;
        UpdatedAt       = DateTimeOffset.UtcNow;
    }

    /// <summary>Incrementa el saldo.</summary>
    /// <exception cref="DomainException">Cantidad no positiva.</exception>
    public void Increase(decimal quantity)
    {
        EnsurePositive(quantity);

        QuantityOnHand += quantity;
        UpdatedAt       = DateTimeOffset.UtcNow;
    }

    /// <summary>Decrementa el saldo, impidiendo stock negativo.</summary>
    /// <exception cref="DomainException">Cantidad no positiva o saldo insuficiente.</exception>
    public void Decrease(decimal quantity)
    {
        EnsurePositive(quantity);

        if (quantity > QuantityOnHand)
            throw new DomainException(
                $"Insufficient stock for product '{ProductPublicId}': on hand {QuantityOnHand}, requested {quantity}.");

        QuantityOnHand -= quantity;
        UpdatedAt       = DateTimeOffset.UtcNow;
    }

    private static void EnsurePositive(decimal quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
    }
}
