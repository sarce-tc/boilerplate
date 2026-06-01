namespace Microservice.Domain.Entities;

/// <summary>Condición frente al IVA del cliente (determina el tipo de comprobante AFIP/ARCA).</summary>
public enum TaxCondition
{
    /// <summary>Consumidor final (default para venta minorista sin identificación fiscal).</summary>
    ConsumidorFinal = 0,

    /// <summary>Responsable Inscripto en IVA.</summary>
    ResponsableInscripto = 1,

    /// <summary>Pequeño contribuyente (Monotributo).</summary>
    Monotributista = 2,

    /// <summary>Sujeto exento de IVA.</summary>
    Exento = 3,

    /// <summary>No responsable / no alcanzado.</summary>
    NoResponsable = 4,
}
