namespace Microservice.Client.Features.CashRegister.Models;

// ── Enums (mirror Microservice.Domain) ───────────────────────────────────────

/// <summary>Mirrors Domain CashSessionStatus.</summary>
public enum CashSessionStatus { Open = 0, Closed = 1 }

/// <summary>Mirrors Domain CashMovementType.</summary>
public enum CashMovementType
{
    Sale = 0, Refund = 1, Deposit = 2, Withdrawal = 3, AdjustmentIn = 4, AdjustmentOut = 5
}

// ── API contract DTOs (mirror Microservice.Application.DTOs.EF) ───────────────

public sealed record CashMovementDto(
    Guid PublicId,
    CashMovementType MovementType,
    decimal Amount,
    decimal SignedAmount,
    string? Description,
    DateTimeOffset CreatedAt);

public sealed record CashSessionDto(
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
    IReadOnlyList<CashMovementDto> Movements);

public sealed record CashSessionsPaginatedDto(
    Guid PublicId,
    string RegisterName,
    CashSessionStatus Status,
    decimal OpeningBalance,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt,
    decimal? Difference);

/// <summary>Body for POST /cash/sessions. Mirrors OpenCashSessionCommand.</summary>
public sealed record OpenCashSessionRequest(string RegisterName, decimal OpeningBalance, string? OpenedBy = null);

/// <summary>Body for POST /cash/sessions/{id}/movements. Mirrors RegisterCashMovementRequestDto.</summary>
public sealed record RegisterCashMovementRequest(CashMovementType MovementType, decimal Amount, string? Description);

/// <summary>Body for POST /cash/sessions/{id}/close. Mirrors CloseCashSessionRequestDto.</summary>
public sealed record CloseCashSessionRequest(decimal DeclaredBalance, string? ClosedBy);
