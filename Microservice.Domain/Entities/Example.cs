using Microservice.Domain.Common;

namespace Microservice.Domain.Entities
{
    public class Example : BaseDomainModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        private Example() { }

        public Example(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", nameof(name));

            Name = name.Trim();
            Description = description?.Trim();
            PublicId = Guid.NewGuid();
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
