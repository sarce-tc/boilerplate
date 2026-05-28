using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.ExistsExample;
// PATRÓN — Verificar existencia con generic-first.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IReadRepository<Example> (Application.Contracts.Persistence.EF): ejecuta ExistsAsync
//     que emite SELECT 1 WHERE predicado sin materializar la entidad, más eficiente que GetEntityAsync + null-check.
public sealed class ExistsExampleQueryHandler(
    IReadRepository<Example> readRepository
    ) : IRequestHandler<ExistsExampleQuery, Result<bool>>
{
    public async Task<Result<bool>> Handle(ExistsExampleQuery request, CancellationToken cancellationToken)
    {
        var exists = await readRepository.ExistsAsync(x => x.PublicId == request.PublicId, cancellationToken);
        return Result<bool>.Success(exists);
    }
}
