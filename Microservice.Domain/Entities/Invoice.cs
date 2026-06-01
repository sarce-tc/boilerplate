using Microservice.Domain.Exceptions;
using Microservice.Domain.ValueObjects;

namespace Microservice.Domain.Entities;

/// <summary>
/// Aggregate root del comprobante electrónico (AFIP/ARCA) emitido por una <see cref="Sale"/>.
/// <para>
/// Nace <see cref="InvoiceStatus.Pending"/>; el gateway de facturación lo lleva a
/// <see cref="InvoiceStatus.Authorized"/> (con CAE y número) o <see cref="InvoiceStatus.Rejected"/>.
/// Referencia a la venta y al cliente por PublicId (sin navegación EF).
/// </para>
/// </summary>
public sealed class Invoice : BaseDomainModel
{
    // ── Constraints ──────────────────────────────────────────────────────────
    public const int CaeMaxLength = 20;
    public const int RejectionReasonMaxLength = 500;

    /// <summary>Venta facturada. Única (un comprobante por venta).</summary>
    public Guid SalePublicId { get; private set; }

    /// <summary>Cliente receptor (opcional: consumidor final).</summary>
    public Guid? CustomerPublicId { get; private set; }

    /// <summary>Tipo de comprobante (A/B/C).</summary>
    public InvoiceType InvoiceType { get; private set; }

    /// <summary>Punto de venta AFIP.</summary>
    public int PointOfSale { get; private set; }

    /// <summary>Número de comprobante asignado por AFIP (null hasta autorizar).</summary>
    public long? InvoiceNumber { get; private set; }

    /// <summary>Estado frente a AFIP.</summary>
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Pending;

    /// <summary>Neto gravado.</summary>
    public decimal Net { get; private set; }

    /// <summary>IVA.</summary>
    public decimal Tax { get; private set; }

    /// <summary>Total del comprobante.</summary>
    public decimal Total { get; private set; }

    /// <summary>Código de Autorización Electrónico devuelto por AFIP.</summary>
    public string? Cae { get; private set; }

    /// <summary>Vencimiento del CAE.</summary>
    public DateTimeOffset? CaeExpiration { get; private set; }

    /// <summary>Momento de autorización.</summary>
    public DateTimeOffset? AuthorizedAt { get; private set; }

    /// <summary>Motivo de rechazo, si aplica.</summary>
    public string? RejectionReason { get; private set; }

    // ── EF Core parameterless constructor (infrastructure only) ──────────────
    private Invoice() { }

    /// <exception cref="ArgumentException">SalePublicId vacío.</exception>
    /// <exception cref="DomainException">Punto de venta no positivo o importes negativos.</exception>
    public Invoice(Guid salePublicId, Guid? customerPublicId, InvoiceType invoiceType, int pointOfSale, decimal net, decimal tax, decimal total)
    {
        if (salePublicId == Guid.Empty)
            throw new ArgumentException("SalePublicId is required.", nameof(salePublicId));
        if (pointOfSale <= 0)
            throw new DomainException("PointOfSale must be greater than zero.");
        if (net < 0 || tax < 0 || total < 0)
            throw new DomainException("Invoice amounts must not be negative.");

        SalePublicId     = salePublicId;
        CustomerPublicId = customerPublicId;
        InvoiceType      = invoiceType;
        PointOfSale      = pointOfSale;
        Net              = net;
        Tax              = tax;
        Total            = total;
        Status           = InvoiceStatus.Pending;
        PublicId         = Guid.NewGuid();
        CreatedAt        = DateTimeOffset.UtcNow;
        UpdatedAt        = DateTimeOffset.UtcNow;
    }

    /// <summary>Registra la autorización de AFIP (CAE + número).</summary>
    /// <exception cref="DomainException">Comprobante no pendiente o CAE vacío.</exception>
    public void Authorize(string cae, DateTimeOffset caeExpiration, long invoiceNumber)
    {
        EnsurePending();
        ArgumentException.ThrowIfNullOrWhiteSpace(cae);

        Cae           = cae.Trim();
        CaeExpiration = caeExpiration;
        InvoiceNumber = invoiceNumber;
        AuthorizedAt  = DateTimeOffset.UtcNow;
        Status        = InvoiceStatus.Authorized;
        UpdatedAt     = DateTimeOffset.UtcNow;
    }

    /// <summary>Registra el rechazo de AFIP.</summary>
    /// <exception cref="DomainException">Comprobante no pendiente.</exception>
    public void Reject(string? reason)
    {
        EnsurePending();

        RejectionReason = reason?.Trim();
        Status          = InvoiceStatus.Rejected;
        UpdatedAt       = DateTimeOffset.UtcNow;
    }

    /// <summary>Resuelve el tipo de comprobante (emisor Responsable Inscripto) según la condición del cliente.</summary>
    public static InvoiceType ResolveType(TaxCondition? customerCondition) => customerCondition switch
    {
        TaxCondition.ResponsableInscripto => InvoiceType.FacturaA,
        _ => InvoiceType.FacturaB,
    };

    private void EnsurePending()
    {
        if (Status != InvoiceStatus.Pending)
            throw new DomainException($"Invoice is already '{Status}'.");
    }
}
