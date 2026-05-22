using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Features.Products.Commands.CreateProduct;
using Microservice.Application.Features.Products.Queries.GetAllProducts;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct(
            [FromBody] CreateProductCommand request,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var query = new GetAllProductsQuery();
            var result = await mediator.Send(query, CancellationToken.None);
            return Ok(result);
        }
    }
}
