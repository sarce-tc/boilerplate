using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler(
        IProductReadRepository readRepository,
        IMapper mapper
    ) : IRequestHandler<GetProductByIdQuery, Result<GetProductByIdDto>>
    {
        public async Task<Result<GetProductByIdDto>> Handle(
            GetProductByIdQuery request, CancellationToken ct)
        {
            var product = await readRepository.GetByPublicIdAsync(request.PublicId, ct);

            if (product is null)
                return Result<GetProductByIdDto>.Failure(Error.NotFound("Product not found"));

            var dto = mapper.Map<GetProductByIdDto>(product);

            return Result<GetProductByIdDto>.Success(dto);
        }
    }
}
