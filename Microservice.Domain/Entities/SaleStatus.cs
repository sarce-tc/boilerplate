namespace Microservice.Domain.Entities;

/// <summary>Estado de una venta del POS.</summary>
public enum SaleStatus
{
    /// <summary>Venta en preparación; admite agregar/quitar ítems.</summary>
    Pending = 0,

    /// <summary>Venta confirmada: stock descontado y cobro registrado en caja. Inmutable.</summary>
    Confirmed = 1,

    /// <summary>Venta anulada antes de confirmar.</summary>
    Cancelled = 2,
}
