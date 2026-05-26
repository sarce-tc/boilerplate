namespace Microservice.Application.DTOs.Customers;

public record CustomerDto(
    Guid            PublicId,
    string          Name,
    string          Email,
    string?         Phone,
    DateTimeOffset  CreatedAt);
