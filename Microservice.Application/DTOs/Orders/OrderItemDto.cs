namespace Microservice.Application.DTOs.Orders;

/// <summary>Read DTO for a single order line item.</summary>
public sealed record OrderItemDto(
    Guid    PublicId,
    string  ProductName,
    int     Quantity,
    decimal UnitPrice,
    decimal LineTotal
);
