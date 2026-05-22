namespace Microservice.Application.DTOs
{
    public record GetAllProductsDto(Guid PublicId, string Name, decimal Price);
}
