using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Domain.Entities;
using Microservice.Application.Contracts.Persistence.EF;

namespace Microservice.Application.Features.ExamplesEF.Queries.CountExamples;
// PATRÓN — Contar registros con generic-first.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IReadRepository<Example> (Application.Contracts.Persistence.EF): ejecuta CountAsync
//     que emite SELECT COUNT(*) sin materializar entidades, apropiado para métricas y cálculos de paginación.
public sealed class CountExamplesQueryHandler(
            IReadRepository<Example> readRepository
    ) : IRequestHandler<CountExamplesQuery, Result<int>>
{
    public async Task<Result<int>> Handle(CountExamplesQuery request, CancellationToken cancellationToken)
    {
        var count = await readRepository.CountAsync(cancellationToken);
        return Result<int>.Success(count);
    }
}
