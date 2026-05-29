using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.Dapper;
// Contrato de escritura Dapper del aggregate Example.
// Extiende IWriteRepository<Example>:
//   · AddAsync  → INSERT del aggregate + INSERT de sus Items hijos (atómico en el TX).
//   · UpdateAsync → solo escalares del aggregate; NO toca los hijos.
//   · UpdateWithItemsAsync → escalares + replace-all de hijos (DELETE + re-INSERT).
//     Específico porque UpdateAsync genérico no puede distinguir "sin hijos" de
//     "no tocar hijos"; el handler elige según request.Items (null vs no-null).
public interface IExampleWriteRepository : IWriteRepository<Example>
{
    Task<Example> UpdateWithItemsAsync(Example entity, CancellationToken ct = default);
}
