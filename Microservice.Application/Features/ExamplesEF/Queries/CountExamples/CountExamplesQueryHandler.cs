using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Domain.Entities;
using Microservice.Application.Contracts.Persistence.EF;

namespace Microservice.Application.Features.ExamplesEF.Queries.CountExamples;
// PATRÓN — Contar registros con generic-first.
// · IReadRepository<T>.CountAsync — no materializa entidades; emite SELECT COUNT(*).
// · Usar para métricas, cálculos de paginación o checks de volumen sin cargar datos.
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
