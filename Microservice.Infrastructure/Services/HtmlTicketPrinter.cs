using System.Globalization;
using System.Text;
using Microservice.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Microservice.Infrastructure.Services;

/// <summary>
/// Implementación de <see cref="ITicketPrinter"/> que renderiza el ticket como HTML imprimible.
/// El encabezado del comercio se lee de configuración (<c>Pos:BusinessName</c>), con valor por defecto.
/// </summary>
public sealed class HtmlTicketPrinter(IConfiguration configuration) : ITicketPrinter
{
    private static readonly CultureInfo Ar = CultureInfo.GetCultureInfo("es-AR");

    public TicketDocument Render(TicketData data)
    {
        var businessName = configuration["Pos:BusinessName"] ?? "Mi Comercio";

        var sb = new StringBuilder();
        sb.Append("<html><head><meta charset=\"utf-8\"><title>Ticket</title></head>");
        sb.Append("<body style=\"font-family:monospace;width:300px\">");
        sb.Append($"<h3 style=\"text-align:center\">{Encode(businessName)}</h3>");
        sb.Append($"<div>Fecha: {data.IssuedAt.ToLocalTime():dd/MM/yyyy HH:mm}</div>");
        sb.Append($"<div>Venta: {data.SalePublicId}</div>");

        if (data.InvoiceNumber is not null)
        {
            sb.Append($"<div>Comprobante {data.InvoiceType}: N° {data.InvoiceNumber}</div>");
            if (!string.IsNullOrWhiteSpace(data.Cae))
                sb.Append($"<div>CAE: {Encode(data.Cae)}</div>");
        }

        if (!string.IsNullOrWhiteSpace(data.CustomerName))
            sb.Append($"<div>Cliente: {Encode(data.CustomerName)} ({Encode(data.CustomerDocNumber)})</div>");

        sb.Append("<hr><table style=\"width:100%\">");
        foreach (var line in data.Lines)
        {
            sb.Append("<tr><td colspan=\"2\">").Append(Encode(line.Description)).Append("</td></tr>");
            sb.Append("<tr><td>")
              .Append(line.Quantity.ToString("0.###", Ar))
              .Append(" x ")
              .Append(Money(line.UnitPrice))
              .Append("</td><td style=\"text-align:right\">")
              .Append(Money(line.LineTotal))
              .Append("</td></tr>");
        }
        sb.Append("</table><hr>");

        sb.Append("<table style=\"width:100%\">");
        sb.Append(Row("Subtotal", data.Subtotal));
        sb.Append(Row("IVA", data.TaxAmount));
        sb.Append($"<tr><td><b>TOTAL</b></td><td style=\"text-align:right\"><b>{Money(data.Total)}</b></td></tr>");
        sb.Append("</table>");

        sb.Append("<p style=\"text-align:center\">¡Gracias por su compra!</p>");
        sb.Append("</body></html>");

        return new TicketDocument("text/html", sb.ToString());
    }

    private static string Row(string label, decimal amount) =>
        $"<tr><td>{label}</td><td style=\"text-align:right\">{Money(amount)}</td></tr>";

    private static string Money(decimal value) => value.ToString("C", Ar);

    private static string Encode(string? value) =>
        System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
}
