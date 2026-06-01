using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Infrastructure;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.InvoicesEF.Commands.IssueInvoice;
// PATRÓN — Operación que combina aggregate + puerto de infraestructura:
//   1. carga la venta CONFIRMADA (tracked) y el cliente (para resolver el tipo de comprobante);
//   2. crea el Invoice (Pending) con los totales de la venta;
//   3. solicita el CAE al gateway AFIP (puerto IElectronicInvoicingService);
//   4. autoriza/rechaza el comprobante y, si fue autorizado, lo asocia a la venta (AttachInvoice);
//   5. un único SaveChangesAsync confirma comprobante + venta.
// Las reglas (venta no confirmada, ya facturada) lanzan DomainException → HTTP 409.
public sealed class IssueInvoiceCommandHandler(
    IReadRepository<Sale> saleRepository,
    IReadRepository<Customer> customerRepository,
    IElectronicInvoicingService invoicingService,
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<IssueInvoiceCommand, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetEntityAsync(
            x => x.PublicId == request.SalePublicId,
            disableTracking: false,
            cancellationToken: cancellationToken);

        if (sale is null)
            return Result<InvoiceDto>.Failure(Error.NotFound($"Sale {request.SalePublicId} not found."));

        // Condición de IVA del cliente → tipo de comprobante (consumidor final si no hay cliente).
        Customer? customer = null;
        if (sale.CustomerPublicId is { } customerId)
            customer = await customerRepository.GetEntityAsync(
                c => c.PublicId == customerId, cancellationToken: cancellationToken);

        var invoiceType = Invoice.ResolveType(customer?.TaxCondition);

        var invoice = new Invoice(
            sale.PublicId,
            sale.CustomerPublicId,
            invoiceType,
            request.PointOfSale,
            sale.Subtotal,
            sale.TaxAmount,
            sale.Total);

        var authRequest = new ElectronicInvoiceRequest(
            invoiceType,
            request.PointOfSale,
            sale.Subtotal,
            sale.TaxAmount,
            sale.Total,
            customer?.DocType,
            customer?.DocNumber);

        var authResult = await invoicingService.RequestAuthorizationAsync(authRequest, cancellationToken);

        if (authResult.Authorized)
        {
            invoice.Authorize(authResult.Cae!, authResult.CaeExpiration!.Value, authResult.InvoiceNumber!.Value);
            // Gancho del vertical 5: vincula el comprobante a la venta (valida que esté confirmada y sin factura previa).
            sale.AttachInvoice(invoice.PublicId);
            unitOfWork.SalesWrite.Update(sale);
        }
        else
        {
            invoice.Reject(authResult.RejectionReason);
        }

        await unitOfWork.InvoicesWrite.AddAsync(invoice, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InvoiceDto>.Success(mapper.Map<InvoiceDto>(invoice));
    }
}
