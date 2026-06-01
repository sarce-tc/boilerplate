namespace Microservice.Domain.Entities;

/// <summary>Tipo de movimiento de inventario. Determina si suma o resta del saldo.</summary>
public enum InventoryMovementType
{
    /// <summary>Ingreso por compra a proveedor (+).</summary>
    Purchase = 0,

    /// <summary>Egreso por venta en el POS (-).</summary>
    Sale = 1,

    /// <summary>Ingreso por devolución de cliente (+).</summary>
    Return = 2,

    /// <summary>Ajuste positivo de inventario, p.ej. conteo físico (+).</summary>
    AdjustmentIn = 3,

    /// <summary>Ajuste negativo de inventario (-).</summary>
    AdjustmentOut = 4,

    /// <summary>Merma, rotura o pérdida (-).</summary>
    Loss = 5,

    /// <summary>Carga inicial de existencias (+).</summary>
    InitialLoad = 6,
}
