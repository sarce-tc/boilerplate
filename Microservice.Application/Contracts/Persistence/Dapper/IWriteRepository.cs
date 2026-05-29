using Microservice.Domain.ValueObjects;

namespace Microservice.Application.Contracts.Persistence.Dapper;
// Contrato de escritura genérico Dapper para cualquier aggregate.
// AddAsync: inserta un registro y retorna la entidad hidratada con id y timestamps generados por la BD.
// UpdateAsync: actualiza un registro y retorna la entidad con campos calculados actualizados.
// DeleteAsync: elimina un registro por su id interno; siempre invocar dentro del bloque IUnitOfWork TX.
public interface IWriteRepository<T> where T : BaseDomainModel
{
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task<T> UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
