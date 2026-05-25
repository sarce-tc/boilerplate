namespace Microservice.Application.DTOs.Orders;

/// <summary>Read DTO for a full Order with its line items.</summary>
public sealed record OrderDto(
    Guid                          PublicId,
    string                        CustomerName,
    string                        Status,
    decimal                       TotalAmount,
    DateTimeOffset                CreatedAt,
    IReadOnlyCollection<OrderItemDto> Items
);
