using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Application.Features.ExamplesDapper.Queries.ExistsExampleByNameDapper;
// PATRÓN — Verifica si existe algún Example con el nombre provisto; retorna true/false sin cargar entidades.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IExampleReadRepository: superficie de lectura Dapper del aggregate
//     Example; expone ExistsByNameAsync para comprobación de unicidad sin carga de datos.
public sealed class ExistsExampleByNameDapperQueryHandler(
    IExampleReadRepository readRepository) : IRequestHandler<ExistsExampleByNameDapperQuery, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        ExistsExampleByNameDapperQuery request, CancellationToken cancellationToken)
    {
        var exists = await readRepository.ExistsByNameAsync(request.Name, cancellationToken);
        return Result<bool>.Success(exists);
    }
}
