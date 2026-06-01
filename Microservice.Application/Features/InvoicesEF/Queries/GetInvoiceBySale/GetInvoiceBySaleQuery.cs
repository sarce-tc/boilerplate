using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.InvoicesEF.Queries.GetInvoiceBySale;
// PATRÓN — Comprobante asociado a una venta.
public record GetInvoiceBySaleQuery(Guid SalePublicId) : IRequest<Result<InvoiceDto>>;
