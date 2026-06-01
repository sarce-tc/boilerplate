using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Models;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CashEF.Queries.GetCashSessionsPaginated;
// PATRÓN — Paginación offset (espejo de GetExamplesPaginatedQueryHandler).
public sealed class GetCashSessionsPaginatedQueryHandler(
    IReadRepository<CashSession> readRepository,
    IMapper mapper
) : IRequestHandler<GetCashSessionsPaginatedQuery, Result<PagedResult<CashSessionsPaginatedDto>>>
{
    public async Task<Result<PagedResult<CashSessionsPaginatedDto>>> Handle(GetCashSessionsPaginatedQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await readRepository.GetListPaginatedAsync(
            request.CurrentPage,
            request.PageSize,
            orderBy: q => q.OrderByDescending(s => s.CreatedAt),
            cancellationToken: cancellationToken);

        var mappedResults = mapper.Map<IEnumerable<CashSessionsPaginatedDto>>(pagedResult.Results);

        var result = new PagedResult<CashSessionsPaginatedDto>(
            mappedResults,
            pagedResult.RowsCount,
            pagedResult.CurrentPage,
            pagedResult.PageSize);

        return Result<PagedResult<CashSessionsPaginatedDto>>.Success(result);
    }
}
