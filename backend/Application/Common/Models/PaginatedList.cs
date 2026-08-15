namespace Application.Common.Models;

public class PaginatedList<T>
{
    public int PgaeNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }

    public IReadOnlyCollection<T>? Items { get; init; }
}
