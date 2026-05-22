namespace Microservice.Application.DTOs
{
    public record ExecuteSqlWithResultDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
}
