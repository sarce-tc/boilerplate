namespace Microservice.Application.DTOs.Customers;

// Class with public setters — Dapper maps directly without reflection tricks
public class CustomerSummaryDto
{
    public Guid           PublicId  { get; set; }
    public string         Name      { get; set; } = string.Empty;
    public string         Email     { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
