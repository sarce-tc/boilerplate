using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;
using Microservice.Application.Services;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleSummary;

// PATRÓN — Query con IExampleService + DTO calculado (sin AutoMapper).
// ── Parámetros ────────────────────────────────────────────────────────────
//   · exampleService — IExampleService (Application.Services): encapsula el lookup estándar por publicId con
//     eager-loading de Items (FindWithItemsAsync) sin que el handler conozca IReadRepository directamente;
//     el aggregate se devuelve con disableTracking:true porque es una query sin persistencia.
public sealed class GetExampleSummaryQueryHandler(
    IExampleService exampleService
) : IRequestHandler<GetExampleSummaryQuery, Result<GetExampleSummaryDto>>
{
    /// <inheritdoc/>
    public async Task<Result<GetExampleSummaryDto>> Handle(
        GetExampleSummaryQuery request, CancellationToken cancellationToken)
    {
        var example = await exampleService.FindWithItemsAsync(request.PublicId, cancellationToken);

        if (example is null)
            return Result<GetExampleSummaryDto>.Failure(
                Error.NotFound($"Example {request.PublicId} not found."));

        var summary = new GetExampleSummaryDto(
            example.PublicId,
            example.Name,
            example.Description,
            example.Status,
            example.Items.Count,
            example.Items.Count(i => i.Status == ExampleItemStatus.Pending),
            example.Items.Count(i => i.Status == ExampleItemStatus.Completed),
            example.CreatedAt,
            example.UpdatedAt);

        return Result<GetExampleSummaryDto>.Success(summary);
    }
}
