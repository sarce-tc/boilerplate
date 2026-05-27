using Microservice.Domain.Common;
using Microservice.Domain.Exceptions;

namespace Microservice.Domain.Entities;

/// <summary>
/// Child entity of the <see cref="Example"/> aggregate root.
/// <para>
/// Cannot be instantiated directly — always created through
/// <see cref="Example.AddItem"/> to preserve aggregate invariants.
/// </para>
/// <para>
/// Persistence: EF Core manages the FK (<c>ExampleId</c>) via relationship
/// fixup; Dapper reads it from the <c>example_items</c> table.
/// </para>
/// </summary>
public sealed class ExampleItem : BaseDomainModel
{
    // ── Constraints ──────────────────────────────────────────────────────────
    /// <summary>Maximum allowed length for <see cref="Label"/>.</summary>
    public const int LabelMaxLength = 150;

    // ── Properties ───────────────────────────────────────────────────────────
    /// <summary>FK to the owning <see cref="Example"/>. Set by EF relationship fixup.</summary>
    public int ExampleId { get; private set; }

    /// <summary>Human-readable item label. Unique within the aggregate. Max <see cref="LabelMaxLength"/> chars.</summary>
    public string Label { get; private set; } = string.Empty;

    /// <summary>Must be greater than zero.</summary>
    public int Quantity { get; private set; }

    /// <summary>Current lifecycle state. Defaults to <see cref="ExampleItemStatus.Pending"/> on creation.</summary>
    public ExampleItemStatus Status { get; private set; } = ExampleItemStatus.Pending;

    // ── EF Core parameterless constructor (infrastructure only) ──────────────
    private ExampleItem() { }

    // ── Package-internal factory (called only by Example.AddItem) ────────────
    internal ExampleItem(string label, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        Label     = label.Trim();
        Quantity  = quantity;
        Status    = ExampleItemStatus.Pending;
        PublicId  = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // ── Domain method (called only by Example.CompleteItem) ──────────────────
    /// <exception cref="DomainException">Thrown when already completed.</exception>
    internal void Complete()
    {
        if (Status == ExampleItemStatus.Completed)
            throw new DomainException($"Item '{Label}' is already completed.");

        Status    = ExampleItemStatus.Completed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
