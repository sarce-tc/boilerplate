using Asp.Versioning;
using MediatR;
using Microservice.API.Extensions;
using Microservice.Application.DTOs;
using Microservice.Application.Features.ExamplesDapper.Queries.GetAllExamplesDapper;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Controllers;
// Controlador Dapper del aggregate Example — expone endpoints HTTP que delegan a MediatR.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · mediator — IMediator (MediatR): despacha queries y commands al handler registrado
//     en el pipeline; desacopla el controller de la capa Application y habilita
//     el flujo de behaviors (validación, logging, caching).
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ExamplesDapperController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// GET /api/v1/examplesdapper/all
    /// Get all Examples via Dapper (unbounded — use only for bounded/reference datasets)
    ///
    /// Returns: 200 OK with all Examples
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<GetAllExamplesDapperDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllExamples(CancellationToken cancellationToken = default)
    {
        var query = new GetAllExamplesDapperQuery();
        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }
}
