using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Models;

namespace Microservice.Application.Features.InvoicesEF.Queries.GetInvoicesPaginated;
// PATRÓN — Listado paginado de comprobantes (más recientes primero).
public record GetInvoicesPaginatedQuery(int CurrentPage = 1, int PageSize = 10)
    : IRequest<Result<PagedResult<InvoicesPaginatedDto>>>;
