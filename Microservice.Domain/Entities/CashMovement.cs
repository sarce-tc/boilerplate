using Microservice.Domain.Exceptions;
using Microservice.Domain.ValueObjects;

namespace Microservice.Domain.Entities;

/// <summary>
/// Entidad hija del aggregate <see cref="CashSession"/>: un movimiento de efectivo del turno.
/// <para>
/// No se instancia directamente — siempre vía <see cref="CashSession.RegisterMovement"/> para
/// preservar las invariantes (la caja debe estar abierta, importe positivo).
/// </para>
/// </summary>
public sealed class CashMovement : BaseDomainModel
{
    // ── Constraints ──────────────────────────────────────────────────────────
    public const int DescriptionMaxLength = 300;

    /// <summary>FK a la <see cref="CashSession"/> propietaria. La fija EF por relationship fixup.</summary>
    public int CashSessionId { get; private set; }

    /// <summary>Tipo de movimiento (define el signo).</summary>
    public CashMovementType MovementType { get; private set; }

    /// <summary>Magnitud del movimiento (siempre positiva).</summary>
    public decimal Amount { get; private set; }

    /// <summary>Detalle libre (p.ej. referencia de la venta).</summary>
    public string? Description { get; private set; }

    /// <summary>Importe con signo según el tipo (+ ingreso / − egreso). No se persiste.</summary>
    public decimal SignedAmount => IsCredit(MovementType) ? Amount : -Amount;

    // ── EF Core parameterless constructor (infrastructure only) ──────────────
    private CashMovement() { }

    // ── Package-internal factory (llamado solo por CashSession.RegisterMovement) ──
    internal CashMovement(CashMovementType movementType, decimal amount, string? description)
    {
        if (amount <= 0)
            throw new DomainException("Movement amount must be greater than zero.");

        MovementType = movementType;
        Amount       = amount;
        Description  = description?.Trim();
        PublicId     = Guid.NewGuid();
        CreatedAt    = DateTimeOffset.UtcNow;
        UpdatedAt    = DateTimeOffset.UtcNow;
    }

    /// <summary>Indica si el tipo de movimiento ingresa (true) o egresa (false) efectivo.</summary>
    public static bool IsCredit(CashMovementType type) => type switch
    {
        CashMovementType.Sale
        or CashMovementType.Deposit
        or CashMovementType.AdjustmentIn => true,
        _ => false,
    };
}
