using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.SalesEF.Commands.CreateSale;
// PATRÓN — Crear aggregate raíz con hijos, tomando snapshot del catálogo:
// por cada ítem carga el Product y fija nombre/precio/IVA en la línea (venta inmutable).
public sealed class CreateSaleCommandHandler(
    IReadRepository<Product> productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateSaleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = new Sale(request.CustomerPublicId, request.CashSessionPublicId);

        foreach (var line in request.Items)
        {
            var product = await productRepository.GetEntityAsync(
                p => p.PublicId == line.ProductPublicId,
                cancellationToken: cancellationToken);

            if (product is null)
                return Result<Guid>.Failure(Error.NotFound($"Product {line.ProductPublicId} not found."));

            sale.AddItem(product.PublicId, product.Name, line.Quantity, product.Price, product.TaxRate);
        }

        await unitOfWork.SalesWrite.AddAsync(sale, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(sale.PublicId);
    }
}
