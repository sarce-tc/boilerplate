using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.CashEF.Queries.GetCashSessionById;
// PATRÓN — Query de aggregate raíz CON hijos (generic-first): GetEntityAsync + includeProperties.
public sealed class GetCashSessionByIdQueryHandler(
    IReadRepository<CashSession> readRepository,
    IMapper mapper
) : IRequestHandler<GetCashSessionByIdQuery, Result<CashSessionDto>>
{
    public async Task<Result<CashSessionDto>> Handle(GetCashSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await readRepository.GetEntityAsync(
            x => x.PublicId == request.PublicId,
            includeProperties: [x => x.Movements],
            cancellationToken: cancellationToken);

        if (session is null)
            return Result<CashSessionDto>.Failure(Error.NotFound($"Cash session {request.PublicId} not found."));

        return Result<CashSessionDto>.Success(mapper.Map<CashSessionDto>(session));
    }
}
