namespace Microservice.Application.DTOs.Orders;

/// <summary>
/// Lightweight projection used by the paginated list endpoint (GET /orders).
/// Populated directly by Dapper from a LEFT JOIN + COUNT aggregate — not mapped
/// from the <c>Order</c> entity because <c>ItemCount</c> is a SQL aggregate.
/// Public setters are required for Dapper property-mapping.
/// </summary>
public sealed class OrderSummaryDto
{
    public Guid            PublicId     { get; set; }
    public string          CustomerName { get; set; } = string.Empty;
    public string          Status       { get; set; } = string.Empty;
    public decimal         TotalAmount  { get; set; }
    public int             ItemCount    { get; set; }
    public DateTimeOffset  CreatedAt    { get; set; }
}
