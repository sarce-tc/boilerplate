using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Models;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CustomersEF.Queries.GetCustomersPaginated;
// PATRÓN — Paginación offset (espejo de GetExamplesPaginatedQueryHandler).
public sealed class GetCustomersPaginatedQueryHandler(
    IReadRepository<Customer> readRepository,
    IMapper mapper
) : IRequestHandler<GetCustomersPaginatedQuery, Result<PagedResult<GetCustomersPaginatedDto>>>
{
    public async Task<Result<PagedResult<GetCustomersPaginatedDto>>> Handle(GetCustomersPaginatedQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await readRepository.GetListPaginatedAsync(
            request.CurrentPage,
            request.PageSize,
            orderBy: q => q.OrderBy(c => c.Name),
            cancellationToken: cancellationToken);

        var mappedResults = mapper.Map<IEnumerable<GetCustomersPaginatedDto>>(pagedResult.Results);

        var result = new PagedResult<GetCustomersPaginatedDto>(
            mappedResults,
            pagedResult.RowsCount,
            pagedResult.CurrentPage,
            pagedResult.PageSize);

        return Result<PagedResult<GetCustomersPaginatedDto>>.Success(result);
    }
}
