using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CustomersEF.Commands.DeleteCustomer;
// PATRÓN — Eliminar aggregate por PublicId (carga tracked → Delete → SaveChanges).
public sealed class DeleteCustomerCommandHandler(
    IReadRepository<Customer> readRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteCustomerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            disableTracking: false,
            cancellationToken: cancellationToken);

        if (customer is null)
            return Result<Guid>.Failure(Error.NotFound($"Customer {request.PublicId} not found."));

        unitOfWork.CustomersWrite.Delete(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(customer.PublicId);
    }
}
