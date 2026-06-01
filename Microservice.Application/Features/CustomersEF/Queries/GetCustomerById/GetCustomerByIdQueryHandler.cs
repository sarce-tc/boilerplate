using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CustomersEF.Queries.GetCustomerById;
// PATRÓN — Query por PublicId (generic-first): GetEntityAsync + map a DTO.
public sealed class GetCustomerByIdQueryHandler(
    IReadRepository<Customer> readRepository,
    IMapper mapper
) : IRequestHandler<GetCustomerByIdQuery, Result<GetCustomerDto>>
{
    public async Task<Result<GetCustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            cancellationToken: cancellationToken);

        if (customer is null)
            return Result<GetCustomerDto>.Failure(Error.NotFound($"Customer {request.PublicId} not found."));

        return Result<GetCustomerDto>.Success(mapper.Map<GetCustomerDto>(customer));
    }
}
