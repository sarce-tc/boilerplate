using Microservice.Domain.Entities;

namespace Microservice.Application.DTOs.EF;

// DTOs de lectura de Gestión de Caja.

/// <summary>Movimiento de efectivo de un turno.</summary>
public record CashMovementDto(
    Guid PublicId,
    CashMovementType MovementType,
    decimal Amount,
    decimal SignedAmount,
    string? Description,
    DateTimeOffset CreatedAt);

/// <summary>Turno de caja con su arqueo y movimientos.</summary>
public record CashSessionDto(
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

/// <summary>Vista liviana para listados paginados (sin movimientos).</summary>
public record CashSessionsPaginatedDto(
    Guid PublicId,
    string RegisterName,
    CashSessionStatus Status,
    decimal OpeningBalance,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt,
    decimal? Difference);

/// <summary>Body de POST /cash/sessions/{publicId}/movements (el turno va en la ruta).</summary>
public record RegisterCashMovementRequestDto(
    CashMovementType MovementType,
    decimal Amount,
    string? Description);

/// <summary>Body de POST /cash/sessions/{publicId}/close.</summary>
public record CloseCashSessionRequestDto(
    decimal DeclaredBalance,
    string? ClosedBy);
