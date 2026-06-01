namespace Microservice.Domain.Entities;

/// <summary>Estado del comprobante electrónico frente a AFIP/ARCA.</summary>
public enum InvoiceStatus
{
    /// <summary>Generado, pendiente de autorización (CAE) por AFIP.</summary>
    Pending = 0,

    /// <summary>Autorizado: CAE obtenido y número asignado.</summary>
    Authorized = 1,

    /// <summary>Rechazado por AFIP.</summary>
    Rejected = 2,
}
