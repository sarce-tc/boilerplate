namespace Microservice.Client.Shared.State;

/// <summary>
/// Base for scoped state containers. Components subscribe to <see cref="Changed"/>
/// and call StateHasChanged; state classes call <see cref="NotifyChanged"/> after a
/// mutation. Kept deliberately tiny (no reducers/actions) — see the architecture
/// decision: scoped containers + CQRS-frontend over Fluxor.
///
/// All mutation methods must be the ONLY way to change observable fields, so the
/// state is unit-testable in isolation (assert on public reads after a method call).
/// </summary>
public abstract class ObservableState
{
    /// <summary>Raised after any state mutation. UI subscribes in OnInitialized, unsubscribes in Dispose.</summary>
    public event Action? Changed;

    protected void NotifyChanged() => Changed?.Invoke();

    /// <summary>Set a backing field and notify only when the value actually changed.</summary>
    protected bool Set<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        NotifyChanged();
        return true;
    }
}
