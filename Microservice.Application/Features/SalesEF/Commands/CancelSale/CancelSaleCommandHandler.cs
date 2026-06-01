using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.SalesEF.Commands.CancelSale;
// PATRÓN — Actualizar aggregate con change tracking: carga tracked → domain method → SaveChanges.
// La regla "solo pendiente" la valida el aggregate → DomainException → HTTP 409.
public sealed class CancelSaleCommandHandler(
    IReadRepository<Sale> readRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CancelSaleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            disableTracking: false,
            cancellationToken: cancellationToken);

        if (sale is null)
            return Result<Guid>.Failure(Error.NotFound($"Sale {request.PublicId} not found."));

        sale.Cancel();

        unitOfWork.SalesWrite.Update(sale);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(sale.PublicId);
    }
}
