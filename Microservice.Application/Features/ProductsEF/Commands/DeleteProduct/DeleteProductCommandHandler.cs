using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ProductsEF.Commands.DeleteProduct;
// PATRÓN — Eliminar aggregate por PublicId (carga tracked → Delete → SaveChanges).
public sealed class DeleteProductCommandHandler(
    IReadRepository<Product> readRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            disableTracking: false,
            cancellationToken: cancellationToken);

        if (product is null)
            return Result<Guid>.Failure(Error.NotFound($"Product {request.PublicId} not found."));

        unitOfWork.ProductsWrite.Delete(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(product.PublicId);
    }
}
