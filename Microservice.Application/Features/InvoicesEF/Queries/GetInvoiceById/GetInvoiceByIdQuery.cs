using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;

namespace Microservice.Application.Features.InvoicesEF.Queries.GetInvoiceById;
// PATRÓN — Detalle de un comprobante por PublicId.
public record GetInvoiceByIdQuery(Guid PublicId) : IRequest<Result<InvoiceDto>>;
