namespace Microservice.Client.Shared.Results;

/// <summary>
/// Discriminated result returned by every gateway/use-case. Mirrors the backend's
/// <c>Result&lt;T&gt;</c> so the UI never deals with raw exceptions or HTTP codes —
/// it pattern-matches on success/failure. Never throws for expected failures.
/// </summary>
public readonly struct UiResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public UiError? Error { get; }

    private UiResult(bool ok, T? value, UiError? error)
    {
        IsSuccess = ok;
        Value = value;
        Error = error;
    }

    public bool IsFailure => !IsSuccess;

    public static UiResult<T> Success(T value) => new(true, value, null);
    public static UiResult<T> Failure(UiError error) => new(false, default, error);

    public static implicit operator UiResult<T>(UiError error) => Failure(error);

    /// <summary>Fold both branches into a single value — keeps components branch-free.</summary>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<UiError, TOut> onError) =>
        IsSuccess ? onSuccess(Value!) : onError(Error!);

    public UiResult<TOut> Map<TOut>(Func<T, TOut> map) =>
        IsSuccess ? UiResult<TOut>.Success(map(Value!)) : UiResult<TOut>.Failure(Error!);
}

/// <summary>Non-generic result for commands that return no payload (just success/failure).</summary>
public readonly struct UiResult
{
    public bool IsSuccess { get; }
    public UiError? Error { get; }

    private UiResult(bool ok, UiError? error)
    {
        IsSuccess = ok;
        Error = error;
    }

    public bool IsFailure => !IsSuccess;

    public static UiResult Success() => new(true, null);
    public static UiResult Failure(UiError error) => new(false, error);

    public static implicit operator UiResult(UiError error) => Failure(error);
}
