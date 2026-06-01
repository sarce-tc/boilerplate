using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Infrastructure;

/// <summary>
/// Puerto de impresión de tickets del POS. Produce el documento imprimible de una venta.
/// <para>
/// La implementación incluida renderiza HTML (multiplataforma, sin hardware). Una impresora
/// térmica ESC/POS implementaría este mismo puerto enviando los comandos al dispositivo.
/// </para>
/// </summary>
public interface ITicketPrinter
{
    /// <summary>Renderiza el ticket a partir de los datos de la venta.</summary>
    TicketDocument Render(TicketData data);
}

/// <summary>Datos necesarios para renderizar el ticket (ensamblados desde el dominio por el handler).</summary>
public record TicketData(
    Guid SalePublicId,
    SaleStatus Status,
    DateTimeOffset IssuedAt,
    string? CustomerName,
    string? CustomerDocNumber,
    IReadOnlyList<TicketLine> Lines,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total,
    InvoiceType? InvoiceType,
    long? InvoiceNumber,
    string? Cae);

/// <summary>Una línea del ticket.</summary>
public record TicketLine(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

/// <summary>Documento imprimible resultante.</summary>
public record TicketDocument(
    string ContentType,
    string Content);
