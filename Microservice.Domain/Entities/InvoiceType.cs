namespace Microservice.Domain.Entities;

/// <summary>Tipo de comprobante AFIP/ARCA según la condición de IVA emisor/receptor.</summary>
public enum InvoiceType
{
    /// <summary>Factura A — emisor RI a receptor Responsable Inscripto.</summary>
    FacturaA = 0,

    /// <summary>Factura B — emisor RI a consumidor final / exento / monotributo.</summary>
    FacturaB = 1,

    /// <summary>Factura C — emisor Monotributista / Exento.</summary>
    FacturaC = 2,
}
