using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ProductsEF.Commands.UpdateProduct;
// PATRÓN — Actualizar aggregate completo con change tracking (espejo de UpdateExampleCommandHandler).
//   · readRepository — carga el Product con includeProperties Barcodes y disableTracking:false
//     para que EF registre los cambios en scalars e hijos automáticamente.
//   · unitOfWork — ProductsWrite.Update marca el aggregate como modificado; SaveChangesAsync confirma.
//   Los cambios se aplican SOLO vía domain methods (UpdateDetails/UpdatePricing/AddBarcode/RemoveBarcode).
public sealed class UpdateProductCommandHandler(
    IReadRepository<Product> readRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            includeProperties: [p => p.Barcodes],
            disableTracking: false,
            cancellationToken: cancellationToken);

        if (product is null)
            return Result<Guid>.Failure(Error.NotFound($"Product {request.PublicId} not found."));

        if (request.Name is not null || request.Description is not null || request.CategoryName is not null)
            product.UpdateDetails(
                request.Name ?? product.Name,
                request.Description ?? product.Description,
                request.CategoryName ?? product.CategoryName);

        if (request.Price is not null || request.Cost is not null || request.TaxRate is not null)
            product.UpdatePricing(
                request.Price ?? product.Price,
                request.Cost ?? product.Cost,
                request.TaxRate ?? product.TaxRate);

        if (request.AddBarcodes is { Count: > 0 })
            foreach (var barcode in request.AddBarcodes)
                product.AddBarcode(barcode.Code, barcode.Symbology);

        if (request.RemoveBarcodeIds is { Count: > 0 })
            foreach (var barcodeId in request.RemoveBarcodeIds)
                product.RemoveBarcode(barcodeId);

        unitOfWork.ProductsWrite.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(product.PublicId);
    }
}
