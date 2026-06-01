using Microservice.Domain.Exceptions;
using Microservice.Domain.ValueObjects;

namespace Microservice.Domain.Entities;

/// <summary>
/// Aggregate root del turno de caja del POS (apertura → movimientos → cierre con arqueo).
/// <para>
/// Posee la colección de <see cref="CashMovement"/>; el balance esperado al cierre se calcula
/// internamente (saldo inicial + suma con signo de los movimientos). Sigue el archetype
/// <see cref="Example"/>/<see cref="ExampleItem"/>.
/// </para>
/// </summary>
public sealed class CashSession : BaseDomainModel
{
    // ── Constraints ──────────────────────────────────────────────────────────
    public const int RegisterNameMaxLength = 80;
    public const int UserMaxLength = 120;

    // ── Properties ───────────────────────────────────────────────────────────
    /// <summary>Identificador de la caja física (p.ej. "Caja 1"). Requerido.</summary>
    public string RegisterName { get; private set; } = string.Empty;

    /// <summary>Estado del turno.</summary>
    public CashSessionStatus Status { get; private set; } = CashSessionStatus.Open;

    /// <summary>Saldo de efectivo declarado al abrir. No negativo.</summary>
    public decimal OpeningBalance { get; private set; }

    /// <summary>Usuario que abrió el turno.</summary>
    public string? OpenedBy { get; private set; }

    /// <summary>Usuario que cerró el turno.</summary>
    public string? ClosedBy { get; private set; }

    /// <summary>Momento del cierre.</summary>
    public DateTimeOffset? ClosedAt { get; private set; }

    /// <summary>Efectivo contado al cierre (declarado por el operador).</summary>
    public decimal? ClosingBalanceDeclared { get; private set; }

    /// <summary>Efectivo esperado al cierre (saldo inicial + movimientos).</summary>
    public decimal? ClosingBalanceExpected { get; private set; }

    /// <summary>Diferencia de arqueo (declarado − esperado). Negativa = faltante.</summary>
    public decimal? Difference { get; private set; }

    // ── Movements (encapsulados — mutados solo por métodos de dominio) ────────
    private readonly List<CashMovement> _movements = [];

    /// <summary>Movimientos del turno. Solo lectura fuera del aggregate.</summary>
    public IReadOnlyList<CashMovement> Movements => _movements.AsReadOnly();

    // ── EF Core parameterless constructor (infrastructure only) ──────────────
    private CashSession() { _movements = []; }

    // ── Factory constructor ──────────────────────────────────────────────────
    /// <exception cref="ArgumentException">RegisterName nulo/vacío.</exception>
    /// <exception cref="DomainException">Saldo inicial negativo.</exception>
    public CashSession(string registerName, decimal openingBalance, string? openedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registerName);
        if (openingBalance < 0)
            throw new DomainException("Opening balance must not be negative.");

        RegisterName   = registerName.Trim();
        OpeningBalance = openingBalance;
        OpenedBy       = openedBy?.Trim();
        Status         = CashSessionStatus.Open;
        PublicId       = Guid.NewGuid();
        CreatedAt      = DateTimeOffset.UtcNow;
        UpdatedAt      = DateTimeOffset.UtcNow;
    }

    // ── Domain methods ───────────────────────────────────────────────────────

    /// <summary>Registra un movimiento de efectivo en el turno abierto.</summary>
    /// <exception cref="DomainException">Caja cerrada o importe no positivo.</exception>
    public CashMovement RegisterMovement(CashMovementType movementType, decimal amount, string? description)
    {
        EnsureOpen();

        var movement = new CashMovement(movementType, amount, description);
        _movements.Add(movement);
        UpdatedAt = DateTimeOffset.UtcNow;
        return movement;
    }

    /// <summary>
    /// Cierra el turno: calcula el efectivo esperado, registra el declarado y la diferencia de arqueo.
    /// </summary>
    /// <exception cref="DomainException">Caja ya cerrada o efectivo declarado negativo.</exception>
    public void Close(decimal declaredBalance, string? closedBy)
    {
        EnsureOpen();
        if (declaredBalance < 0)
            throw new DomainException("Declared balance must not be negative.");

        var expected = OpeningBalance + _movements.Sum(m => m.SignedAmount);

        ClosingBalanceExpected = expected;
        ClosingBalanceDeclared = declaredBalance;
        Difference             = declaredBalance - expected;
        ClosedBy               = closedBy?.Trim();
        ClosedAt               = DateTimeOffset.UtcNow;
        Status                 = CashSessionStatus.Closed;
        UpdatedAt              = DateTimeOffset.UtcNow;
    }

    /// <summary>Efectivo esperado en este instante (saldo inicial + movimientos con signo).</summary>
    public decimal CurrentBalance() => OpeningBalance + _movements.Sum(m => m.SignedAmount);

    // ── Guard ────────────────────────────────────────────────────────────────
    private void EnsureOpen()
    {
        if (Status != CashSessionStatus.Open)
            throw new DomainException("Cannot operate on a closed cash session.");
    }
}
