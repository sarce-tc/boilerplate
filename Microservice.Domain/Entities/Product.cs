using Microservice.Domain.Common;

namespace Microservice.Domain.Entities
{
    public sealed class Product : BaseDomainModel
    {
        public string Name { get; private set; }
        public decimal Price { get; private set; }

        private Product() { }

        public static Product Create(string name, decimal price)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            if (price <= 0) throw new ArgumentException("Price must be positive");

            return new Product { PublicId = Guid.NewGuid(), Name = name, Price = price };
        }
    }
}
