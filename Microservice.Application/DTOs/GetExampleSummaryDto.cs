using Microservice.Domain.Entities;

namespace Microservice.Application.DTOs;

// Contrato de salida de la query GetExampleSummary.
// El handler lo construye manualmente desde la entidad Example con Items cargados, calculando conteos
// de ítems pendientes y completados; no pasa por AutoMapper porque contiene campos calculados.
public record GetExampleSummaryDto(
    Guid PublicId,
    string Name,
    string? Description,
    ExampleStatus Status,
    int ItemCount,
    int PendingItemCount,
    int CompletedItemCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
