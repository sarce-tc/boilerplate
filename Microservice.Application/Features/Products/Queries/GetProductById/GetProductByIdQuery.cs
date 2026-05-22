using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.Products.Queries.GetProductById
{
    public record GetProductByIdQuery(Guid PublicId) : IRequest<Result<GetProductByIdDto>>;
}
