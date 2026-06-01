using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ProductsEF.Queries.GetProductByBarcode;
// PATRÓN — Lectura por predicado sobre la colección de hijos (generic-first):
// EF traduce p.Barcodes.Any(b => b.Code == code) a un EXISTS; sin repo específico.
public sealed class GetProductByBarcodeQueryHandler(
    IReadRepository<Product> readRepository,
    IMapper mapper
) : IRequestHandler<GetProductByBarcodeQuery, Result<GetProductDto>>
{
    public async Task<Result<GetProductDto>> Handle(GetProductByBarcodeQuery request, CancellationToken cancellationToken)
    {
        var product = await readRepository.GetEntityAsync(
            x => x.Barcodes.Any(b => b.Code == request.Code),
            includeProperties: [x => x.Barcodes],
            cancellationToken: cancellationToken);

        if (product is null)
            return Result<GetProductDto>.Failure(Error.NotFound($"No product found for barcode '{request.Code}'."));

        return Result<GetProductDto>.Success(mapper.Map<GetProductDto>(product));
    }
}
