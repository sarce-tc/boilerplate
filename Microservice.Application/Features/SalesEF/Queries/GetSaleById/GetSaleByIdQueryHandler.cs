using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.SalesEF.Queries.GetSaleById;
// PATRÓN — Query de aggregate raíz CON hijos (generic-first): GetEntityAsync + includeProperties.
public sealed class GetSaleByIdQueryHandler(
    IReadRepository<Sale> readRepository,
    IMapper mapper
) : IRequestHandler<GetSaleByIdQuery, Result<SaleDto>>
{
    public async Task<Result<SaleDto>> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            includeProperties: [x => x.Items],
            cancellationToken: cancellationToken);

        if (sale is null)
            return Result<SaleDto>.Failure(Error.NotFound($"Sale {request.PublicId} not found."));

        return Result<SaleDto>.Success(mapper.Map<SaleDto>(sale));
    }
}
