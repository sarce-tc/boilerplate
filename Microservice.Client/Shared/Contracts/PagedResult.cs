using System.Text.Json.Serialization;

namespace Microservice.Client.Shared.Contracts;

/// <summary>
/// Exact mirror of the backend <c>PagedResult&lt;T&gt;</c>
/// (Microservice.Application.Models.PagedResult). Field names match the JSON wire
/// contract — do not rename. Used as the API DTO; features map it to a UI page model.
/// </summary>
public sealed class PagedResult<T>
{
    [JsonPropertyName("results")] public IReadOnlyList<T> Results { get; set; } = [];
    [JsonPropertyName("rowsCount")] public int RowsCount { get; set; }
    [JsonPropertyName("pageCount")] public int PageCount { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
    [JsonPropertyName("currentPage")] public int CurrentPage { get; set; }

    public static PagedResult<T> Empty(int page = 1, int size = 10) =>
        new() { Results = [], RowsCount = 0, PageCount = 0, PageSize = size, CurrentPage = page };
}
