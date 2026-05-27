using Microservice.Domain.Common;
using Microservice.Domain.Exceptions;

namespace Microservice.Domain.Entities;

/// <summary>
/// Reference aggregate root — demonstrates the full DDD + EF Core path:
/// private setters, domain invariants via <see cref="DomainException"/>,
/// and state-machine transitions through explicit domain methods.
/// <para>
/// Persistence: <c>IWriteRepository&lt;Example&gt;</c> / <c>IReadRepository&lt;Example&gt;</c>
/// coordinated by <c>IUnitOfWork.SaveChangesAsync</c>.
/// </para>
/// </summary>
public sealed class Example : BaseDomainModel
{
    // ── Constraints ──────────────────────────────────────────────────────────
    /// <summary>Maximum allowed length for <see cref="Name"/>.</summary>
    public const int NameMaxLength = 200;

    /// <summary>Maximum allowed length for <see cref="Description"/>.</summary>
    public const int DescriptionMaxLength = 1_000;

    // ── Properties ───────────────────────────────────────────────────────────
    /// <summary>Human-readable identifier. Required; max <see cref="NameMaxLength"/> chars.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Optional free-text details. Max <see cref="DescriptionMaxLength"/> chars.</summary>
    public string? Description { get; private set; }

    /// <summary>Current lifecycle state. Defaults to <see cref="ExampleStatus.Active"/> on creation.</summary>
    public ExampleStatus Status { get; private set; } = ExampleStatus.Active;

    // ── Items collection (encapsulated — mutated only through domain methods) ─
    private readonly List<ExampleItem> _items = [];

    /// <summary>Child items owned by this aggregate. Read-only outside the aggregate.</summary>
    public IReadOnlyList<ExampleItem> Items => _items.AsReadOnly();

    // ── EF Core parameterless constructor (infrastructure only) ──────────────
    private Example() { _items = []; }

    // ── Factory constructor ──────────────────────────────────────────────────
    /// <summary>
    /// Creates a new <see cref="Example"/> in the <see cref="ExampleStatus.Active"/> state.
    /// </summary>
    /// <param name="name">Required. Trimmed and stored as-is.</param>
    /// <param name="description">Optional free-text details.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public Example(string name, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name        = name.Trim();
        Description = description?.Trim();
        Status      = ExampleStatus.Active;
        PublicId    = Guid.NewGuid();
        CreatedAt   = DateTimeOffset.UtcNow;
        UpdatedAt   = DateTimeOffset.UtcNow;
    }

    // ── Domain methods ───────────────────────────────────────────────────────

    /// <summary>
    /// Replaces the current name.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    /// <exception cref="DomainException">Thrown when the aggregate is <see cref="ExampleStatus.Inactive"/>.</exception>
    public void UpdateName(string name)
    {
        EnsureActive();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name      = name.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Replaces the current description. Pass <see langword="null"/> to clear it.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the aggregate is <see cref="ExampleStatus.Inactive"/>.</exception>
    public void UpdateDescription(string? description)
    {
        EnsureActive();

        Description = description?.Trim();
        UpdatedAt   = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Transitions the aggregate to <see cref="ExampleStatus.Inactive"/>.
    /// </summary>
    /// <exception cref="DomainException">Thrown when already inactive.</exception>
    public void Deactivate()
    {
        if (Status == ExampleStatus.Inactive)
            throw new DomainException("Example is already inactive.");

        Status    = ExampleStatus.Inactive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Transitions the aggregate back to <see cref="ExampleStatus.Active"/>.
    /// </summary>
    /// <exception cref="DomainException">Thrown when already active.</exception>
    public void Activate()
    {
        if (Status == ExampleStatus.Active)
            throw new DomainException("Example is already active.");

        Status    = ExampleStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // ── Item management ──────────────────────────────────────────────────────

    /// <summary>
    /// Adds a new <see cref="ExampleItem"/> to this aggregate.
    /// Labels are unique (case-insensitive) within the same example.
    /// </summary>
    /// <returns>The newly created item.</returns>
    /// <exception cref="DomainException">
    /// Thrown when the aggregate is inactive or a duplicate label exists.
    /// </exception>
    public ExampleItem AddItem(string label, int quantity)
    {
        EnsureActive();
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        var trimmed = label.Trim();

        if (_items.Exists(i => i.Label.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"An item with label '{trimmed}' already exists.");

        var item = new ExampleItem(trimmed, quantity);
        _items.Add(item);
        UpdatedAt = DateTimeOffset.UtcNow;
        return item;
    }

    /// <summary>
    /// Removes an item by its public identifier.
    /// </summary>
    /// <exception cref="DomainException">
    /// Thrown when the aggregate is inactive or the item is not found.
    /// </exception>
    public void RemoveItem(Guid itemPublicId)
    {
        EnsureActive();

        var item = _items.Find(i => i.PublicId == itemPublicId)
            ?? throw new DomainException($"Item '{itemPublicId}' not found in this example.");

        _items.Remove(item);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Transitions an item to <see cref="ExampleItemStatus.Completed"/>.
    /// </summary>
    /// <exception cref="DomainException">
    /// Thrown when the item is not found or is already completed.
    /// </exception>
    public void CompleteItem(Guid itemPublicId)
    {
        var item = _items.Find(i => i.PublicId == itemPublicId)
            ?? throw new DomainException($"Item '{itemPublicId}' not found in this example.");

        item.Complete();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // ── Guard ────────────────────────────────────────────────────────────────
    private void EnsureActive()
    {
        if (Status != ExampleStatus.Active)
            throw new DomainException("Cannot modify an inactive example.");
    }
}
