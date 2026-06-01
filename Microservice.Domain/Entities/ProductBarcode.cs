using Microservice.Domain.Exceptions;
using Microservice.Domain.ValueObjects;

namespace Microservice.Domain.Entities;

/// <summary>
/// Entidad hija del aggregate <see cref="Product"/>: representa un código de barras escaneable.
/// <para>
/// No se instancia directamente — siempre vía <see cref="Product.AddBarcode"/> para preservar
/// las invariantes del aggregate (unicidad del código dentro del producto).
/// </para>
/// </summary>
public sealed class ProductBarcode : BaseDomainModel
{
    // ── Constraints ──────────────────────────────────────────────────────────
    public const int CodeMaxLength = 64;
    public const int SymbologyMaxLength = 32;

    // ── Properties ───────────────────────────────────────────────────────────
    /// <summary>FK al <see cref="Product"/> propietario. La fija EF por relationship fixup.</summary>
    public int ProductId { get; private set; }

    /// <summary>Contenido del código escaneado (EAN-13, UPC, etc.). Único; max <see cref="CodeMaxLength"/>.</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>Simbología opcional del código (EAN13, CODE128, QR…). Max <see cref="SymbologyMaxLength"/>.</summary>
    public string? Symbology { get; private set; }

    // ── EF Core parameterless constructor (infrastructure only) ──────────────
    private ProductBarcode() { }

    // ── Package-internal factory (llamado solo por Product.AddBarcode) ───────
    internal ProductBarcode(string code, string? symbology)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        Code      = code.Trim();
        Symbology = symbology?.Trim();
        PublicId  = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
