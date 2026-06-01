namespace Microservice.Client.Features.CashRegister.Models;

/// <summary>Bound by the open-session form.</summary>
public sealed class OpenSessionFormModel
{
    public string RegisterName { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public string? OpenedBy { get; set; }

    public OpenCashSessionRequest ToRequest() => new(RegisterName.Trim(), OpeningBalance, OpenedBy);
}

/// <summary>Bound by the register-movement form.</summary>
public sealed class MovementFormModel
{
    public CashMovementType MovementType { get; set; } = CashMovementType.Deposit;
    public decimal Amount { get; set; }
    public string? Description { get; set; }

    public RegisterCashMovementRequest ToRequest() => new(MovementType, Amount, Description);
}

/// <summary>Bound by the close (arqueo) dialog.</summary>
public sealed class CloseSessionFormModel
{
    public decimal DeclaredBalance { get; set; }
    public string? ClosedBy { get; set; }

    public CloseCashSessionRequest ToRequest() => new(DeclaredBalance, ClosedBy);
}
