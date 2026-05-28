using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Services;

/// <summary>
/// Default implementation of <see cref="IExampleService"/>.
/// Depends only on the generic <see cref="IReadRepository{T}"/> — no infrastructure import required.
/// </summary>
public sealed class ExampleService(IReadRepository<Example> readRepository) : IExampleService
{
    /// <inheritdoc/>
    public Task<Example?> FindAsync(Guid publicId, CancellationToken ct = default) =>
        readRepository.GetEntityAsync(
            x => x.PublicId == publicId,
            cancellationToken: ct);

    /// <inheritdoc/>
    public Task<Example?> FindWithItemsAsync(Guid publicId, CancellationToken ct = default) =>
        readRepository.GetEntityAsync(
            x => x.PublicId == publicId,
            includeProperties: [e => e.Items],
            cancellationToken: ct);

    /// <inheritdoc/>
    public Task<Example?> FindTrackedAsync(Guid publicId, CancellationToken ct = default) =>
        readRepository.GetEntityAsync(
            x => x.PublicId == publicId,
            disableTracking: false,
            cancellationToken: ct);
}
