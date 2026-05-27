namespace Microservice.Domain.Entities;

/// <summary>
/// Lifecycle states for an <see cref="ExampleItem"/> within its aggregate.
/// Transitions are enforced by <see cref="Example"/> domain methods.
/// </summary>
public enum ExampleItemStatus
{
    Pending   = 1,
    Completed = 2
}
