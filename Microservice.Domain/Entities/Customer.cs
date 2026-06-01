using Microservice.Domain.Exceptions;
using Microservice.Domain.ValueObjects;

namespace Microservice.Domain.Entities;

/// <summary>
/// Aggregate root de clientes del POS. Mantiene datos fiscales (tipo/número de documento,
/// condición frente al IVA) requeridos por la facturación electrónica AFIP/ARCA, más datos de contacto.
/// Sigue el archetype <see cref="Example"/> (sin hijos).
/// </summary>
public sealed class Customer : BaseDomainModel
{
    // ── Constraints ──────────────────────────────────────────────────────────
    public const int NameMaxLength = 200;
    public const int DocNumberMaxLength = 20;
    public const int EmailMaxLength = 256;
    public const int PhoneMaxLength = 40;
    public const int AddressMaxLength = 300;

    // ── Properties ───────────────────────────────────────────────────────────
    /// <summary>Razón social o nombre completo. Requerido.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Tipo de documento fiscal.</summary>
    public DocumentType DocType { get; private set; }

    /// <summary>Número de documento. Requerido; max <see cref="DocNumberMaxLength"/>.</summary>
    public string DocNumber { get; private set; } = string.Empty;

    /// <summary>Condición frente al IVA (determina el tipo de comprobante).</summary>
    public TaxCondition TaxCondition { get; private set; }

    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }

    /// <summary>Indica si el cliente está habilitado para operar.</summary>
    public bool IsActive { get; private set; }

    // ── EF Core parameterless constructor (infrastructure only) ──────────────
    private Customer() { }

    // ── Factory constructor ──────────────────────────────────────────────────
    public Customer(
        string name,
        DocumentType docType,
        string docNumber,
        TaxCondition taxCondition,
        string? email,
        string? phone,
        string? address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(docNumber);

        Name         = name.Trim();
        DocType      = docType;
        DocNumber    = docNumber.Trim();
        TaxCondition = taxCondition;
        Email        = email?.Trim();
        Phone        = phone?.Trim();
        Address      = address?.Trim();
        IsActive     = true;
        PublicId     = Guid.NewGuid();
        CreatedAt    = DateTimeOffset.UtcNow;
        UpdatedAt    = DateTimeOffset.UtcNow;
    }

    // ── Domain methods ───────────────────────────────────────────────────────

    public void UpdateName(string name)
    {
        EnsureActive();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name      = name.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Actualiza los datos fiscales (tipo/número de documento y condición de IVA).</summary>
    public void UpdateFiscalData(DocumentType docType, string docNumber, TaxCondition taxCondition)
    {
        EnsureActive();
        ArgumentException.ThrowIfNullOrWhiteSpace(docNumber);

        DocType      = docType;
        DocNumber    = docNumber.Trim();
        TaxCondition = taxCondition;
        UpdatedAt    = DateTimeOffset.UtcNow;
    }

    /// <summary>Actualiza los datos de contacto. Pasar null limpia el campo.</summary>
    public void UpdateContact(string? email, string? phone, string? address)
    {
        EnsureActive();

        Email     = email?.Trim();
        Phone     = phone?.Trim();
        Address   = address?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Customer is already inactive.");

        IsActive  = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("Customer is already active.");

        IsActive  = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // ── Guard ────────────────────────────────────────────────────────────────
    private void EnsureActive()
    {
        if (!IsActive)
            throw new DomainException("Cannot modify an inactive customer.");
    }
}
