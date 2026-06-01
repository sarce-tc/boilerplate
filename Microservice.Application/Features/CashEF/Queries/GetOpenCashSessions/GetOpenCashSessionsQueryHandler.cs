using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CashEF.Queries.GetOpenCashSessions;
// PATRÓN — Lectura de colección por predicado (generic-first): GetListAsync(status == Open).
public sealed class GetOpenCashSessionsQueryHandler(
    IReadRepository<CashSession> readRepository,
    IMapper mapper
) : IRequestHandler<GetOpenCashSessionsQuery, Result<IReadOnlyList<CashSessionsPaginatedDto>>>
{
    public async Task<Result<IReadOnlyList<CashSessionsPaginatedDto>>> Handle(GetOpenCashSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await readRepository.GetListAsync(
            predicate: s => s.Status == CashSessionStatus.Open,
            orderBy: q => q.OrderByDescending(s => s.CreatedAt),
            cancellationToken: cancellationToken);

        var dtos = mapper.Map<IReadOnlyList<CashSessionsPaginatedDto>>(sessions);
        return Result<IReadOnlyList<CashSessionsPaginatedDto>>.Success(dtos);
    }
}
