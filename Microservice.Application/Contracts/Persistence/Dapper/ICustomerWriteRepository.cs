using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.Dapper;

public interface ICustomerWriteRepository : IWriteRepository<Customer>
{
}
