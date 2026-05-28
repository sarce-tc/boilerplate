using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.Dapper;
// Contrato de escritura Dapper del aggregate Example.
// Extiende IWriteRepository<Example>; la superficie genérica (AddAsync, UpdateAsync, DeleteAsync)
// cubre todas las operaciones actuales del aggregate.
public interface IExampleWriteRepository : IWriteRepository<Example>
{
}
