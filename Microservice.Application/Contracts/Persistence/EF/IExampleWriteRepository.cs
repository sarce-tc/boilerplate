using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.EF;
// Contrato de escritura EF Core del aggregate Example.
// Extiende IWriteRepository<Example>; la superficie genérica cubre todas las operaciones actuales.
// Agregar métodos específicos aquí solo cuando IWriteRepository<T> no sea suficiente.
public interface IExampleWriteRepository : IWriteRepository<Example>
{
}
