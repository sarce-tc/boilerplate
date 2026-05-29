using Microservice.Application.DTOs.Dapper;
using Microservice.Application.Models;
using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.Dapper;
// Contrato de lectura Dapper del aggregate Example.
// Extiende IReadRepository<Example> con operaciones específicas del dominio.
//
// Dos familias de lectura:
//   · Entidad (IReadRepository<Example>): superficie genérica que hidrata el aggregate
//     plano (sin hijos) — la usan los write/update handlers para cargar y mutar.
//   · DTO con hijos (*WithItemsAsync): read models proyectados por JOIN + multi-mapping
//     a dapper.example_items. NO hidratan el aggregate (Items es read-only en dominio);
//     devuelven DTOs listos para la respuesta.
public interface IExampleReadRepository : IReadRepository<Example>
{
    Task<IReadOnlyList<Example>> SearchByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);

    // ── Reads con hijos (DTO + multi-mapping) ────────────────────────────────
    Task<GetExampleByPublicIdDto?> GetByPublicIdWithItemsAsync(Guid publicId, CancellationToken ct = default);
    Task<PagedResult<GetExamplesPaginatedDto>> GetPaginatedWithItemsAsync(int currentPage, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<GetAllExamplesDto>> GetAllWithItemsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SearchExamplesByNameDto>> SearchByNameWithItemsAsync(string name, CancellationToken ct = default);
}
