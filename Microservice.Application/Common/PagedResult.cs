namespace Microservice.Application.Common;

/// <summary>
/// Generic wrapper for paginated read results returned by list endpoints.
/// Produced by Dapper paged queries (QueryMultipleAsync) and returned by list query handlers.
/// </summary>
/// <param name="Items">The items on the current page.</param>
/// <param name="TotalCount">Total number of records across all pages.</param>
/// <param name="Page">Current 1-based page number.</param>
/// <param name="PageSize">Maximum records per page.</param>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int              TotalCount,
    int              Page,
    int              PageSize)
{
    public int  TotalPages      => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage     => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
