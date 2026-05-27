using Microservice.Domain.Entities;
using Microservice.Domain.Exceptions;

namespace Microservice.Domain.Services;

/// <summary>
/// Default implementation of <see cref="IExampleDomainService"/>.
/// Pure domain logic — no I/O, no infrastructure dependencies.
/// </summary>
public sealed class ExampleDomainService : IExampleDomainService
{
    /// <inheritdoc/>
    public void TransferItem(Example source, Example destination, Guid itemPublicId)
    {
        // ── 1. Locate item (read-only; aggregate enforces existence on Remove) ─
        var item = source.Items.FirstOrDefault(i => i.PublicId == itemPublicId)
            ?? throw new DomainException($"Item '{itemPublicId}' not found in source example '{source.PublicId}'.");

        // ── 2. Add to destination first — enforces destination invariants ──────
        //       (active state, unique label) before touching source.
        destination.AddItem(item.Label, item.Quantity);

        // ── 3. Remove from source — enforces source active-state invariant ─────
        source.RemoveItem(itemPublicId);
    }

    /// <inheritdoc/>
    public void MergeInto(Example source, Example destination)
    {
        var pendingItems = source.Items
            .Where(i => i.Status == ExampleItemStatus.Pending)
            .ToList();

        // ── 1. Validate label conflicts before mutating either aggregate ───────
        //       Keeps the operation all-or-nothing at the domain level.
        var conflicts = pendingItems
            .Where(i => destination.Items.Any(
                d => d.Label.Equals(i.Label, StringComparison.OrdinalIgnoreCase)))
            .Select(i => i.Label)
            .ToList();

        if (conflicts.Count > 0)
            throw new DomainException(
                $"Cannot merge: label conflict(s) in destination — {string.Join(", ", conflicts)}.");

        // ── 2. Transfer all pending items ─────────────────────────────────────
        foreach (var item in pendingItems)
            destination.AddItem(item.Label, item.Quantity);

        // ── 3. Deactivate source ──────────────────────────────────────────────
        source.Deactivate();
    }
}
