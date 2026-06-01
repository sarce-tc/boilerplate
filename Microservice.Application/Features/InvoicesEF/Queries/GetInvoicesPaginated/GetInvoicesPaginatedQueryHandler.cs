using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Models;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.InvoicesEF.Queries.GetInvoicesPaginated;
// PATRÓN — Paginación offset (espejo de GetExamplesPaginatedQueryHandler).
public sealed class GetInvoicesPaginatedQueryHandler(
    IReadRepository<Invoice> readRepository,
    IMapper mapper
) : IRequestHandler<GetInvoicesPaginatedQuery, Result<PagedResult<InvoicesPaginatedDto>>>
{
    public async Task<Result<PagedResult<InvoicesPaginatedDto>>> Handle(GetInvoicesPaginatedQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await readRepository.GetListPaginatedAsync(
            request.CurrentPage,
            request.PageSize,
            orderBy: q => q.OrderByDescending(i => i.CreatedAt),
            cancellationToken: cancellationToken);

        var mappedResults = mapper.Map<IEnumerable<InvoicesPaginatedDto>>(pagedResult.Results);

        var result = new PagedResult<InvoicesPaginatedDto>(
            mappedResults,
            pagedResult.RowsCount,
            pagedResult.CurrentPage,
            pagedResult.PageSize);

        return Result<PagedResult<InvoicesPaginatedDto>>.Success(result);
    }
}
