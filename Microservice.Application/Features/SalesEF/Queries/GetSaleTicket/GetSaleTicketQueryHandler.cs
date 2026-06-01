using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Infrastructure;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.SalesEF.Queries.GetSaleTicket;
// PATRÓN — Lectura multi-aggregate + puerto de infraestructura:
//   carga la venta (con ítems), el cliente y el comprobante asociado (si existen),
//   ensambla TicketData y delega el render en ITicketPrinter. Sin escritura.
public sealed class GetSaleTicketQueryHandler(
    IReadRepository<Sale> saleRepository,
    IReadRepository<Customer> customerRepository,
    IReadRepository<Invoice> invoiceRepository,
    ITicketPrinter ticketPrinter
) : IRequestHandler<GetSaleTicketQuery, Result<TicketDocument>>
{
    public async Task<Result<TicketDocument>> Handle(GetSaleTicketQuery request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetEntityAsync(
            x => x.PublicId == request.SalePublicId,
            includeProperties: [s => s.Items],
            cancellationToken: cancellationToken);

        if (sale is null)
            return Result<TicketDocument>.Failure(Error.NotFound($"Sale {request.SalePublicId} not found."));

        Customer? customer = null;
        if (sale.CustomerPublicId is { } customerId)
            customer = await customerRepository.GetEntityAsync(
                c => c.PublicId == customerId, cancellationToken: cancellationToken);

        var invoice = await invoiceRepository.GetEntityAsync(
            i => i.SalePublicId == sale.PublicId, cancellationToken: cancellationToken);

        var lines = sale.Items
            .Select(i => new TicketLine(i.ProductName, i.Quantity, i.UnitPrice, i.LineTotal))
            .ToList();

        var data = new TicketData(
            sale.PublicId,
            sale.Status,
            sale.ConfirmedAt ?? sale.CreatedAt,
            customer?.Name,
            customer?.DocNumber,
            lines,
            sale.Subtotal,
            sale.TaxAmount,
            sale.Total,
            invoice?.InvoiceType,
            invoice?.InvoiceNumber,
            invoice?.Cae);

        return Result<TicketDocument>.Success(ticketPrinter.Render(data));
    }
}
