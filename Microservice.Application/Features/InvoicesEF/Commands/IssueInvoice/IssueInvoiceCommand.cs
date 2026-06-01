using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.InvoicesEF.Commands.IssueInvoice;
// PATRÓN — Emite el comprobante electrónico de una venta confirmada (solicita CAE al gateway AFIP).
public record IssueInvoiceCommand(Guid SalePublicId, int PointOfSale = 1) : IRequest<Result<InvoiceDto>>;
