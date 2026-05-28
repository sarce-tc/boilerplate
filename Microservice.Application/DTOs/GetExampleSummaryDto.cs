using Microservice.Domain.Entities;

namespace Microservice.Application.DTOs;

/// <summary>
/// Lightweight aggregate summary: scalar fields + computed item-count statistics.
/// No child collection is included — use <c>GetExampleWithItemsDto</c> when full item detail is needed.
/// </summary>
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
