using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Products.Commands.CreateProduct
{
    public record CreateProductCommand(string Name, decimal Price) : IRequest<Result<Guid>>;
}
