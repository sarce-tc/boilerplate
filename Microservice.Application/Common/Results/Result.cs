namespace Microservice.Application.Common.Results;

/// <summary>
/// Result pattern implementation for functional error handling
/// 
/// Use Case: Represent success or failure without exceptions
/// 
/// Pattern Benefits:
/// - No exception overhead
/// - Explicit success/failure handling
/// - Structured error information
/// - Composable error handling
/// 
/// Usage:
/// var result = Result.Success();
/// var result = Result.Failure(Error.Validation("Invalid input"));
/// var result = Result<int>.Success(42);
/// var result = Result<int>.Failure(Error.NotFound("User not found"));
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool IsSuccess { get; protected set; }

    /// <summary>
    /// List of errors if operation failed
    /// </summary>
    public List<Error> Errors { get; protected set; } = [];

    protected Result(bool isSuccess, List<Error>? errors = null)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? [];
    }

    /// <summary>
    /// Create successful result
    /// </summary>
    public static Result Success() 
        => new(true);

    /// <summary>
    /// Create failed result with single error
    /// </summary>
    public static Result Failure(Error error) 
        => new(false, [error]);

    /// <summary>
    /// Create failed result with multiple errors
    /// </summary>
    public static Result Failure(List<Error> errors) 
        => new(false, errors);

    /// <summary>
    /// Create failed result from validation failures
    /// </summary>
    public static Result FailureFromValidation(List<string> messages)
    {
        var errors = messages.Select(m => Error.Validation(m)).ToList();
        return new(false, errors);
    }
}

/// <summary>
/// Generic result pattern with data payload
/// 
/// Use Case: Return data on success, errors on failure
/// 
/// Example:
/// var result = await GetUser(id);
/// if (result.IsSuccess)
///     return result.Value;
/// </summary>
public class Result<T> : Result
{
    /// <summary>
    /// Data payload on success (null if failed)
    /// </summary>
    public T? Value { get; protected set; }

    protected Result(bool isSuccess, T? value = default, List<Error>? errors = null)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    /// <summary>
    /// Create successful result with data
    /// </summary>
    public static Result<T> Success(T value) 
        => new(true, value);

    /// <summary>
    /// Create failed result with single error
    /// </summary>
    public static new Result<T> Failure(Error error) 
        => new(false, errors: [error]);

    /// <summary>
    /// Create failed result with multiple errors
    /// </summary>
    public static new Result<T> Failure(List<Error> errors) 
        => new(false, errors: errors);

    /// <summary>
    /// Create failed result from validation failures
    /// </summary>
    public static new Result<T> FailureFromValidation(List<string> messages)
    {
        var errors = messages.Select(m => Error.Validation(m)).ToList();
        return new(false, errors: errors);
    }
}
