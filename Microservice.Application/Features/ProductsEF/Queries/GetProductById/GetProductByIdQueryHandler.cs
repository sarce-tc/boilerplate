using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ProductsEF.Queries.GetProductById;
// PATRÓN — Query de aggregate raíz CON hijos (generic-first): GetEntityAsync + includeProperties.
public sealed class GetProductByIdQueryHandler(
    IReadRepository<Product> readRepository,
    IMapper mapper
) : IRequestHandler<GetProductByIdQuery, Result<GetProductDto>>
{
    public async Task<Result<GetProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            includeProperties: [x => x.Barcodes],
            cancellationToken: cancellationToken);

        if (product is null)
            return Result<GetProductDto>.Failure(Error.NotFound($"Product {request.PublicId} not found."));

        return Result<GetProductDto>.Success(mapper.Map<GetProductDto>(product));
    }
}
