namespace Microservice.Domain.Entities;

/// <summary>
/// Lifecycle states for the <see cref="Example"/> aggregate.
/// Transitions are enforced by domain methods; never set directly.
/// </summary>
public enum ExampleStatus
{
    Active   = 1,
    Inactive = 2
}
