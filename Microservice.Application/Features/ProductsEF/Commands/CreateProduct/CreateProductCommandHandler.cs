using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ProductsEF.Commands.CreateProduct;
// PATRÓN — Crear aggregate raíz con hijos inicial (espejo de CreateExampleCommandHandler).
//   · unitOfWork.ProductsWrite — superficie genérica de escritura (LINQRepository<Product>).
//   · mapper — proyecta el command → Product vía factory constructor (invariantes garantizadas).
//   Los códigos de barras se agregan con el domain method AddBarcode (valida unicidad/activo).
public sealed class CreateProductCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = mapper.Map<Product>(request);

        if (request.Barcodes is { Count: > 0 })
            foreach (var barcode in request.Barcodes)
                product.AddBarcode(barcode.Code, barcode.Symbology);

        await unitOfWork.ProductsWrite.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(product.PublicId);
    }
}
