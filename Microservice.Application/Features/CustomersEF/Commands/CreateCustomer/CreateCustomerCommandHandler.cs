using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CustomersEF.Commands.CreateCustomer;
// PATRÓN — Crear aggregate sin hijos (espejo de CreateExampleCommandHandler).
public sealed class CreateCustomerCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = mapper.Map<Customer>(request);

        await unitOfWork.CustomersWrite.AddAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(customer.PublicId);
    }
}
