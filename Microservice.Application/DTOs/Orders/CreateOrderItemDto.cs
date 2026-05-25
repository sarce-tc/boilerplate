namespace Microservice.Application.DTOs.Orders;

/// <summary>Input DTO for a single line item when creating an Order.</summary>
public sealed record CreateOrderItemDto(
    string  ProductName,
    int     Quantity,
    decimal UnitPrice
);
