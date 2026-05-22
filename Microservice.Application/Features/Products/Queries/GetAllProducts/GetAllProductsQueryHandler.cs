using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.Products.Queries.GetAllProducts
{
    // Microservice.Application/Features/Products/Queries/GetAllProducts/GetAllProductsQueryHandler.cs
    public class GetAllProductsQueryHandler(
        IProductReadRepository readRepository,
        IMapper mapper
    ) : IRequestHandler<GetAllProductsQuery, Result<IReadOnlyList<GetAllProductsDto>>>
    {
        public async Task<Result<IReadOnlyList<GetAllProductsDto>>> Handle(
            GetAllProductsQuery request, CancellationToken ct)
        {
            var products = await readRepository.GetAllAsync(ct);
            var data = mapper.Map<IReadOnlyList<GetAllProductsDto>>(products);
            return Result<IReadOnlyList<GetAllProductsDto>>.Success(data);
        }
    }
}
