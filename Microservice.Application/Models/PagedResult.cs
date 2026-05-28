namespace Microservice.Application.Models;
public class PagedResult<T> where T : class
{
    public IEnumerable<T> Results { get; set; } = [];
    public int RowsCount { get; set; }
    public int PageCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }

    public PagedResult(IEnumerable<T> results, int rowsCount, int currentPage, int pageSize)
    {
        Results = results ?? [];
        RowsCount = rowsCount;
        PageSize = pageSize;
        CurrentPage = currentPage;
        PageCount = (int)Math.Ceiling(rowsCount / (double)pageSize);
    }
}
