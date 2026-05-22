using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.Products.Queries.GetAllProducts
{
    // Microservice.Application/Features/Products/Queries/GetAllProducts/GetAllProductsQuery.cs
    public record GetAllProductsQuery : IRequest<Result<IReadOnlyList<GetAllProductsDto>>>;
}
