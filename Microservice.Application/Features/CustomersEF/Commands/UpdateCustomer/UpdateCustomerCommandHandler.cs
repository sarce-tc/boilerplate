using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CustomersEF.Commands.UpdateCustomer;
// PATRÓN — Actualizar aggregate con change tracking (espejo de UpdateExampleCommandHandler).
// Cambios SOLO vía domain methods; un único SaveChangesAsync confirma la TX implícita.
public sealed class UpdateCustomerCommandHandler(
    IReadRepository<Customer> readRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateCustomerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            disableTracking: false,
            cancellationToken: cancellationToken);

        if (customer is null)
            return Result<Guid>.Failure(Error.NotFound($"Customer {request.PublicId} not found."));

        if (request.Name is not null)
            customer.UpdateName(request.Name);

        if (request.DocType is not null || request.DocNumber is not null || request.TaxCondition is not null)
            customer.UpdateFiscalData(
                request.DocType ?? customer.DocType,
                request.DocNumber ?? customer.DocNumber,
                request.TaxCondition ?? customer.TaxCondition);

        if (request.Email is not null || request.Phone is not null || request.Address is not null)
            customer.UpdateContact(
                request.Email ?? customer.Email,
                request.Phone ?? customer.Phone,
                request.Address ?? customer.Address);

        unitOfWork.CustomersWrite.Update(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(customer.PublicId);
    }
}
