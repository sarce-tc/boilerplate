using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CustomersEF.Queries.GetCustomerByDocument;
// PATRÓN — Lectura por predicado (generic-first): GetEntityAsync(c => c.DocNumber == n).
public sealed class GetCustomerByDocumentQueryHandler(
    IReadRepository<Customer> readRepository,
    IMapper mapper
) : IRequestHandler<GetCustomerByDocumentQuery, Result<GetCustomerDto>>
{
    public async Task<Result<GetCustomerDto>> Handle(GetCustomerByDocumentQuery request, CancellationToken cancellationToken)
    {
        var customer = await readRepository.GetEntityAsync(
            x => x.DocNumber == request.DocNumber,
            cancellationToken: cancellationToken);

        if (customer is null)
            return Result<GetCustomerDto>.Failure(Error.NotFound($"No customer found for document '{request.DocNumber}'."));

        return Result<GetCustomerDto>.Success(mapper.Map<GetCustomerDto>(customer));
    }
}
