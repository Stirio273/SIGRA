namespace SIGRA.Controllers;

public record CreateTicketRequest(string Name, decimal Price);
public record UpdateTicketRequest(string Name, decimal Price);
public record TicketResponse(int Id, string Name, decimal Price, DateTime CreatedAt);

public class PagedRequest
{
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = new List<T>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

