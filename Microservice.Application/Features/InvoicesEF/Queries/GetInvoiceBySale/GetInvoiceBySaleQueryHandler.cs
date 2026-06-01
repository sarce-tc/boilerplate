using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.InvoicesEF.Queries.GetInvoiceBySale;
// PATRÓN — Lectura por predicado (generic-first): GetEntityAsync(i => i.SalePublicId == id).
public sealed class GetInvoiceBySaleQueryHandler(
    IReadRepository<Invoice> readRepository,
    IMapper mapper
) : IRequestHandler<GetInvoiceBySaleQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceBySaleQuery request, CancellationToken cancellationToken)
    {
        var invoice = await readRepository.GetEntityAsync(
            x => x.SalePublicId == request.SalePublicId,
            cancellationToken: cancellationToken);

        if (invoice is null)
            return Result<InvoiceDto>.Failure(Error.NotFound($"No invoice for sale '{request.SalePublicId}'."));

        return Result<InvoiceDto>.Success(mapper.Map<InvoiceDto>(invoice));
    }
}
