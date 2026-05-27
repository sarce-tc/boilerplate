using Microservice.Domain.Entities;

namespace Microservice.Domain.Services;

/// <summary>
/// Domain service for cross-aggregate operations on <see cref="Example"/> aggregates.
/// <para>
/// Operations that do not naturally belong to a single aggregate root — because they
/// require coordinating two or more aggregates — live here instead of on the entity.
/// </para>
/// <para>
/// The implementation is pure domain logic (no I/O); persistence atomicity is the
/// caller's responsibility via <c>IUnitOfWork</c>.
/// </para>
/// </summary>
public interface IExampleDomainService
{
    /// <summary>
    /// Moves a <see cref="ExampleItem"/> from <paramref name="source"/> to
    /// <paramref name="destination"/>, enforcing the invariants of both aggregates.
    /// </summary>
    /// <exception cref="Exceptions.DomainException">
    /// Thrown when either aggregate is inactive, the item is not found in
    /// <paramref name="source"/>, or <paramref name="destination"/> already contains
    /// an item with the same label.
    /// </exception>
    void TransferItem(Example source, Example destination, Guid itemPublicId);

    /// <summary>
    /// Copies all <see cref="ExampleItemStatus.Pending"/> items from
    /// <paramref name="source"/> into <paramref name="destination"/>, then
    /// deactivates <paramref name="source"/>.
    /// The operation is all-or-nothing within the domain: it validates label conflicts
    /// before mutating either aggregate.
    /// </summary>
    /// <exception cref="Exceptions.DomainException">
    /// Thrown when either aggregate is inactive or label conflicts exist.
    /// </exception>
    void MergeInto(Example source, Example destination);
}
