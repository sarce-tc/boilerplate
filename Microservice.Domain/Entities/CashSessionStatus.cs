namespace Microservice.Domain.Entities;

/// <summary>Estado del turno de caja.</summary>
public enum CashSessionStatus
{
    /// <summary>Caja abierta, admite movimientos.</summary>
    Open = 0,

    /// <summary>Caja cerrada (arqueo realizado), inmutable.</summary>
    Closed = 1,
}
