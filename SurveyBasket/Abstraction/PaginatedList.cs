public class PaginatedList<T>
{
    public List<T> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int TotalPages { get; init; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList() { }

    private PaginatedList(
        List<T> items,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        Items = items;
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await source.CountAsync(cancellationToken);

        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
    }
}
