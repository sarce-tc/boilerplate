namespace Microservice.Client.Shared.Contracts;

/// <summary>Paging parameters matching the backend query string (?page=&amp;size=).</summary>
public readonly record struct PageRequest(int Page = 1, int Size = 10)
{
    public string ToQueryString() => $"page={Page}&size={Size}";
}
