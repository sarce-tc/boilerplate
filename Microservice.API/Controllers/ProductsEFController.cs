using Asp.Versioning;
using MediatR;
using Microservice.API.Extensions;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Features.ProductsEF.Commands.CreateProduct;
using Microservice.Application.Features.ProductsEF.Commands.DeleteProduct;
using Microservice.Application.Features.ProductsEF.Commands.UpdateProduct;
using Microservice.Application.Features.ProductsEF.Queries.GetProductByBarcode;
using Microservice.Application.Features.ProductsEF.Queries.GetProductById;
using Microservice.Application.Features.ProductsEF.Queries.GetProductsPaginated;
using Microservice.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Controllers;
// Controlador EF Core del aggregate Product — catálogo de productos del POS.
// Thin controller: cada acción despacha un command/query vía MediatR y traduce Result → IActionResult.
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ProductsEFController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// POST /api/products
    /// Crea un producto (con códigos de barras opcionales).
    /// Returns: 201 Created con el PublicId · 400 si la validación falla
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    /// <summary>
    /// GET /api/products/{publicId}
    /// Detalle del producto con sus códigos de barras.
    /// Returns: 200 OK · 404 si no existe
    /// </summary>
    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(GetProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductByIdQuery(publicId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/products/by-barcode/{code}
    /// Resolución de escaneo: devuelve el producto asociado al código leído.
    /// Returns: 200 OK · 404 si ningún producto tiene ese código
    /// </summary>
    [HttpGet("by-barcode/{code}")]
    [ProducesResponseType(typeof(GetProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductByBarcode(
        string code,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductByBarcodeQuery(code), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/products?page=1&amp;size=10
    /// Listado paginado del catálogo.
    /// Returns: 200 OK con PagedResult
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<GetProductsPaginatedDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetProductsPaginatedQuery(page, size), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// PUT /api/products/{publicId}
    /// Actualiza datos, precios y códigos de barras del producto.
    /// Returns: 200 OK · 404 si no existe · 409 si el producto está inactivo
    /// </summary>
    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProduct(
        Guid publicId,
        [FromBody] UpdateProductRequestDto? request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            publicId,
            request?.Name,
            request?.Description,
            request?.CategoryName,
            request?.Price,
            request?.Cost,
            request?.TaxRate,
            request?.AddBarcodes,
            request?.RemoveBarcodeIds);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// DELETE /api/products/{publicId}
    /// Elimina el producto (códigos de barras en cascada).
    /// Returns: 200 OK · 404 si no existe
    /// </summary>
    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteProductCommand(publicId), cancellationToken);
        return result.ToActionResult();
    }
}
