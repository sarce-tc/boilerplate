using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.ExistsExample
{
    // PATRÓN — Verificar existencia con generic-first.
    // · IReadRepository<T>.ExistsAsync — emite SELECT 1 WHERE ...; no carga la entidad.
    // · Preferir sobre GetEntityAsync + null-check cuando solo se necesita saber si existe.
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
}
