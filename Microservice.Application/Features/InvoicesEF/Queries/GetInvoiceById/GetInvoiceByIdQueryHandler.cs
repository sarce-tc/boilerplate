using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.InvoicesEF.Queries.GetInvoiceById;
// PATRÓN — Lectura por predicado (generic-first).
public sealed class GetInvoiceByIdQueryHandler(
    IReadRepository<Invoice> readRepository,
    IMapper mapper
) : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            cancellationToken: cancellationToken);

        if (invoice is null)
            return Result<InvoiceDto>.Failure(Error.NotFound($"Invoice {request.PublicId} not found."));

        return Result<InvoiceDto>.Success(mapper.Map<InvoiceDto>(invoice));
    }
}
