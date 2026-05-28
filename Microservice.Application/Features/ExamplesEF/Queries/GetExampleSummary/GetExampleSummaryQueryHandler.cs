using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;
using Microservice.Application.Services;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleSummary;

// PATRÓN — Query con IExampleService + DTO calculado (sin AutoMapper).
// ── Decisiones de diseño de referencia ────────────────────────────────────
//   · IExampleService.FindWithItemsAsync encapsula el lookup + includeProperties:[Items]
//     sin que el handler conozca IReadRepository<Example> directamente.
//   · El DTO contiene campos calculados (ItemCount, PendingItemCount, CompletedItemCount)
//     que AutoMapper no puede proyectar sin configuración adicional; construir el
//     record manualmente es más claro y no requiere un profile extra.
//   · disableTracking:true (default de FindWithItemsAsync) — es una query, no hay SaveChanges.
// ── Cuándo usar IExampleService en lugar de IReadRepository<T> directamente ─
//   Cuando el lookup es el estándar "por publicId, opcionalmente con hijos" y no
//   necesita un predicado personalizado ni proyección. Si la query requiere ILike,
//   paginación o ThenInclude anidado, inyectar IReadRepository<T> o IExampleReadRepository.
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
