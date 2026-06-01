namespace Microservice.Client.Features.CashRegister.Models;

/// <summary>Lightweight view of an open cash session — what the POS needs to attach a sale.</summary>
public sealed record CashSessionSummaryVm(
    Guid PublicId,
    string RegisterName,
    decimal OpeningBalance,
    DateTimeOffset OpenedAt,
    bool IsOpen);

/// <summary>A cash movement row for the session detail.</summary>
public sealed record CashMovementVm(
    Guid PublicId,
    CashMovementType MovementType,
    decimal Amount,
    decimal SignedAmount,
    string? Description,
    DateTimeOffset CreatedAt);

/// <summary>Full session detail with movements and a running balance projection.</summary>
public sealed record CashSessionDetailVm(
    Guid PublicId,
    string RegisterName,
    CashSessionStatus Status,
    decimal OpeningBalance,
    string? OpenedBy,
    DateTimeOffset OpenedAt,
    string? ClosedBy,
    DateTimeOffset? ClosedAt,
    decimal? ClosingBalanceDeclared,
    decimal? ClosingBalanceExpected,
    decimal? Difference,
    IReadOnlyList<CashMovementVm> Movements)
{
    public bool IsOpen => Status == CashSessionStatus.Open;

    /// <summary>Opening + signed movements — the live expected cash in the drawer.</summary>
    public decimal CurrentBalance => OpeningBalance + Movements.Sum(m => m.SignedAmount);
}

/// <summary>Result of the arqueo (close): expected vs declared and the difference.</summary>
public sealed record CashCloseResultVm(
    decimal Expected,
    decimal Declared,
    decimal Difference)
{
    public bool IsBalanced => Difference == 0m;
    public bool IsShortage => Difference < 0m; // declared < expected (falta efectivo)
}

/// <summary>Explicit mapping for the CashRegister feature (archetype-consistent, no reflection).</summary>
public static class CashMapper
{
    public static CashSessionSummaryVm ToSummary(CashSessionsPaginatedDto dto) =>
        new(dto.PublicId, dto.RegisterName, dto.OpeningBalance, dto.OpenedAt, dto.Status == CashSessionStatus.Open);

    public static CashSessionSummaryVm ToSummary(CashSessionDto dto) =>
        new(dto.PublicId, dto.RegisterName, dto.OpeningBalance, dto.OpenedAt, dto.Status == CashSessionStatus.Open);

    public static CashMovementVm ToMovement(CashMovementDto dto) =>
        new(dto.PublicId, dto.MovementType, dto.Amount, dto.SignedAmount, dto.Description, dto.CreatedAt);

    public static CashSessionDetailVm ToDetail(CashSessionDto dto) => new(
        dto.PublicId, dto.RegisterName, dto.Status, dto.OpeningBalance, dto.OpenedBy, dto.OpenedAt,
        dto.ClosedBy, dto.ClosedAt, dto.ClosingBalanceDeclared, dto.ClosingBalanceExpected, dto.Difference,
        dto.Movements.Select(ToMovement).ToList());

    /// <summary>Project a closed session into the arqueo result (expected/declared from the close).</summary>
    public static CashCloseResultVm ToCloseResult(CashSessionDto dto) => new(
        Expected: dto.ClosingBalanceExpected ?? 0m,
        Declared: dto.ClosingBalanceDeclared ?? 0m,
        Difference: dto.Difference ?? 0m);
}
