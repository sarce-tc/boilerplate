namespace Microservice.Application.DTOs
{
    public record GetProductByIdDto(Guid PublicId, string Name, decimal Price);
}
