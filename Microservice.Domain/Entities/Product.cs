using Microservice.Domain.Exceptions;
using Microservice.Domain.ValueObjects;

namespace Microservice.Domain.Entities;

/// <summary>
/// Aggregate root del catálogo de productos del POS.
/// <para>
/// Invariantes vía <see cref="DomainException"/>: SKU/Nombre obligatorios, precios no negativos,
/// alícuota de IVA en rango [0,100], códigos de barras únicos dentro del producto.
/// Sigue el archetype <see cref="Example"/> (setters privados, factory constructor, métodos de dominio).
/// </para>
/// </summary>
public sealed class Product : BaseDomainModel
{
    // ── Constraints ──────────────────────────────────────────────────────────
    public const int SkuMaxLength = 64;
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 1_000;
    public const int CategoryMaxLength = 120;

    // ── Properties ───────────────────────────────────────────────────────────
    /// <summary>Código interno único del producto. Requerido; max <see cref="SkuMaxLength"/>.</summary>
    public string Sku { get; private set; } = string.Empty;

    /// <summary>Nombre comercial. Requerido; max <see cref="NameMaxLength"/>.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Detalle opcional. Max <see cref="DescriptionMaxLength"/>.</summary>
    public string? Description { get; private set; }

    /// <summary>Precio de venta (con o sin IVA según política comercial). No negativo.</summary>
    public decimal Price { get; private set; }

    /// <summary>Costo de reposición. No negativo.</summary>
    public decimal Cost { get; private set; }

    /// <summary>Alícuota de IVA aplicable (porcentaje, p.ej. 21.00). Rango [0,100].</summary>
    public decimal TaxRate { get; private set; }

    /// <summary>Categoría opcional para agrupar el catálogo.</summary>
    public string? CategoryName { get; private set; }

    /// <summary>Indica si el producto está habilitado para la venta.</summary>
    public bool IsActive { get; private set; }

    // ── Barcodes (encapsulados — mutados solo por métodos de dominio) ─────────
    private readonly List<ProductBarcode> _barcodes = [];

    /// <summary>Códigos de barras del producto. Solo lectura fuera del aggregate.</summary>
    public IReadOnlyList<ProductBarcode> Barcodes => _barcodes.AsReadOnly();

    // ── EF Core parameterless constructor (infrastructure only) ──────────────
    private Product() { _barcodes = []; }

    // ── Factory constructor ──────────────────────────────────────────────────
    /// <exception cref="ArgumentException">Sku o Name nulos/vacíos.</exception>
    /// <exception cref="DomainException">Precio/costo negativos o alícuota fuera de rango.</exception>
    public Product(
        string sku,
        string name,
        string? description,
        decimal price,
        decimal cost,
        decimal taxRate,
        string? categoryName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureMoney(price, nameof(price));
        EnsureMoney(cost, nameof(cost));
        EnsureTaxRate(taxRate);

        Sku          = sku.Trim();
        Name         = name.Trim();
        Description  = description?.Trim();
        Price        = price;
        Cost         = cost;
        TaxRate      = taxRate;
        CategoryName = categoryName?.Trim();
        IsActive     = true;
        PublicId     = Guid.NewGuid();
        CreatedAt    = DateTimeOffset.UtcNow;
        UpdatedAt    = DateTimeOffset.UtcNow;
    }

    // ── Domain methods ───────────────────────────────────────────────────────

    /// <summary>Actualiza datos descriptivos del producto.</summary>
    public void UpdateDetails(string name, string? description, string? categoryName)
    {
        EnsureActive();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name         = name.Trim();
        Description  = description?.Trim();
        CategoryName = categoryName?.Trim();
        UpdatedAt    = DateTimeOffset.UtcNow;
    }

    /// <summary>Actualiza precio de venta, costo y alícuota de IVA.</summary>
    public void UpdatePricing(decimal price, decimal cost, decimal taxRate)
    {
        EnsureActive();
        EnsureMoney(price, nameof(price));
        EnsureMoney(cost, nameof(cost));
        EnsureTaxRate(taxRate);

        Price     = price;
        Cost      = cost;
        TaxRate   = taxRate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <exception cref="DomainException">Cuando ya está inactivo.</exception>
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Product is already inactive.");

        IsActive  = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <exception cref="DomainException">Cuando ya está activo.</exception>
    public void Activate()
    {
        if (IsActive)
            throw new DomainException("Product is already active.");

        IsActive  = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // ── Barcode management ─────────────────────────────────────────────────────

    /// <summary>Agrega un código de barras. Único (case-insensitive) dentro del producto.</summary>
    /// <exception cref="DomainException">Producto inactivo o código duplicado.</exception>
    public ProductBarcode AddBarcode(string code, string? symbology)
    {
        EnsureActive();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var trimmed = code.Trim();

        if (_barcodes.Exists(b => b.Code.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Barcode '{trimmed}' already exists on this product.");

        var barcode = new ProductBarcode(trimmed, symbology);
        _barcodes.Add(barcode);
        UpdatedAt = DateTimeOffset.UtcNow;
        return barcode;
    }

    /// <exception cref="DomainException">Producto inactivo o código no encontrado.</exception>
    public void RemoveBarcode(Guid barcodePublicId)
    {
        EnsureActive();

        var barcode = _barcodes.Find(b => b.PublicId == barcodePublicId)
            ?? throw new DomainException($"Barcode '{barcodePublicId}' not found on this product.");

        _barcodes.Remove(barcode);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // ── Guards ─────────────────────────────────────────────────────────────────
    private void EnsureActive()
    {
        if (!IsActive)
            throw new DomainException("Cannot modify an inactive product.");
    }

    private static void EnsureMoney(decimal value, string field)
    {
        if (value < 0)
            throw new DomainException($"{field} must not be negative.");
    }

    private static void EnsureTaxRate(decimal taxRate)
    {
        if (taxRate is < 0 or > 100)
            throw new DomainException("TaxRate must be between 0 and 100.");
    }
}
