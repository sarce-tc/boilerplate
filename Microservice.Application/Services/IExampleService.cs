using Microservice.Domain.Entities;

namespace Microservice.Application.Services;

/// <summary>
/// Application service for common <see cref="Example"/> lookup operations.
/// <para>
/// Encapsulates the repeated "find by publicId" pattern so handlers stay thin.
/// Inject this service instead of <c>IReadRepository&lt;Example&gt;</c> when only a
/// standard lookup is needed — no custom predicate, no projection.
/// </para>
/// <para>
/// <b>Tracking contract:</b>
/// <list type="bullet">
///   <item><term>FindAsync / FindWithItemsAsync</term><description>Read-only (disableTracking: true). For query handlers.</description></item>
///   <item><term>FindTrackedAsync</term><description>Change-tracking on (disableTracking: false). For command handlers that call domain methods and persist via <c>IUnitOfWork</c>.</description></item>
/// </list>
/// </para>
/// </summary>
public interface IExampleService
{
    /// <summary>
    /// Returns the <see cref="Example"/> with the given <paramref name="publicId"/>,
    /// or <see langword="null"/> if not found. Untracked — use in query handlers.
    /// </summary>
    Task<Example?> FindAsync(Guid publicId, CancellationToken ct = default);

    /// <summary>
    /// Returns the <see cref="Example"/> with its <see cref="Example.Items"/> collection
    /// eagerly loaded, or <see langword="null"/> if not found. Untracked — use in query handlers
    /// that need to inspect or project children.
    /// </summary>
    Task<Example?> FindWithItemsAsync(Guid publicId, CancellationToken ct = default);

    /// <summary>
    /// Returns the <see cref="Example"/> with change-tracking enabled, or
    /// <see langword="null"/> if not found. Use in command handlers that call domain methods
    /// and then persist via <c>IUnitOfWork.SaveChangesAsync</c>.
    /// </summary>
    Task<Example?> FindTrackedAsync(Guid publicId, CancellationToken ct = default);
}
