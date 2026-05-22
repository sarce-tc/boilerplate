namespace Microservice.Application.DTOs
{
    public record GetExampleByIdDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
}
