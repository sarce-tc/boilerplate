using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.InventoryEF.Queries.GetInventoryMovementsByProduct;
// PATRÓN — Lectura de colección por predicado + orden (generic-first): GetListAsync.
public sealed class GetInventoryMovementsByProductQueryHandler(
    IReadRepository<InventoryMovement> readRepository,
    IMapper mapper
) : IRequestHandler<GetInventoryMovementsByProductQuery, Result<IReadOnlyList<InventoryMovementDto>>>
{
    public async Task<Result<IReadOnlyList<InventoryMovementDto>>> Handle(GetInventoryMovementsByProductQuery request, CancellationToken cancellationToken)
    {
        var movements = await readRepository.GetListAsync(
            predicate: m => m.ProductPublicId == request.ProductPublicId,
            orderBy: q => q.OrderByDescending(m => m.CreatedAt),
            cancellationToken: cancellationToken);

        var dtos = mapper.Map<IReadOnlyList<InventoryMovementDto>>(movements);
        return Result<IReadOnlyList<InventoryMovementDto>>.Success(dtos);
    }
}
