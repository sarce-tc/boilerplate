namespace Microservice.Domain.Entities;

/// <summary>Tipo de movimiento de caja. Determina si ingresa (+) o egresa (-) efectivo.</summary>
public enum CashMovementType
{
    /// <summary>Cobro de una venta (+).</summary>
    Sale = 0,

    /// <summary>Devolución a cliente (-).</summary>
    Refund = 1,

    /// <summary>Ingreso de efectivo a la caja (+).</summary>
    Deposit = 2,

    /// <summary>Retiro de efectivo de la caja (-).</summary>
    Withdrawal = 3,

    /// <summary>Ajuste positivo de arqueo (+).</summary>
    AdjustmentIn = 4,

    /// <summary>Ajuste negativo de arqueo (-).</summary>
    AdjustmentOut = 5,
}
