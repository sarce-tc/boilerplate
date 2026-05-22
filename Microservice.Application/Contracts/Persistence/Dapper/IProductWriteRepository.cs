using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Persistence.Dapper
{
    public interface IProductWriteRepository : IWriteRepository<Product>
    {
        // Métodos específicos de escritura si aparecen en el futuro
    }
}
