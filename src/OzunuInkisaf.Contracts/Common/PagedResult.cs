namespace OzunuInkisaf.Contracts.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class ApiError
{
    public string Message { get; set; } = string.Empty;

    public IReadOnlyDictionary<string, string[]>? Errors { get; set; }
}
