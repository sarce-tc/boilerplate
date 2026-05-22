namespace Microservice.Application.DTOs
{
    public record GetExamplesFromSqlDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
}
