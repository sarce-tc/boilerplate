namespace Microservice.Application.Common.Results;

// ═══════════════════════════════════════════════════════════════════════════
// AGENT — Result pattern. Use instead of throwing exceptions in handlers.
//
// Commands (no return value):  return Result.Success() / Result.Failure(Error.X(...))
// Queries  (with data):        return Result<T>.Success(value) / Result<T>.Failure(Error.X(...))
//
// In controllers: result.ToActionResult() or result.ToActionResult(StatusCodes.Status201Created)
// ═══════════════════════════════════════════════════════════════════════════

public class Result
{
    public bool        IsSuccess { get; protected set; }
    public List<Error> Errors    { get; protected set; } = [];

    protected Result(bool isSuccess, List<Error>? errors = null)
    {
        IsSuccess = isSuccess;
        Errors    = errors ?? [];
    }

    public static Result Success()            => new(true);
    public static Result Failure(Error error) => new(false, [error]);
    public static Result Failure(List<Error> errors) => new(false, errors);

    public static Result FailureFromValidation(List<string> messages)
        => new(false, messages.Select(m => Error.Validation(m)).ToList());
}

public class Result<T> : Result
{
    public T? Value { get; protected set; }

    protected Result(bool isSuccess, T? value = default, List<Error>? errors = null)
        : base(isSuccess, errors) => Value = value;

    public static Result<T> Success(T value)            => new(true,  value);
    public static new Result<T> Failure(Error error)    => new(false, errors: [error]);
    public static new Result<T> Failure(List<Error> errors) => new(false, errors: errors);

    public static new Result<T> FailureFromValidation(List<string> messages)
        => new(false, errors: messages.Select(m => Error.Validation(m)).ToList());
}
