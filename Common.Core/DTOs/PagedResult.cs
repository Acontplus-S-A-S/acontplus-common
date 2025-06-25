namespace Common.Core.DTOs;

/// <summary>
/// Generic class for paginated results
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PagedResult<T> where T : class
{
    /// <summary>
    /// Items in the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = [];

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// Size of a page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total count of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Flag indicating if there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// Flag indicating if there is a next page
    /// </summary>
    public bool HasNextPage => PageIndex < TotalPages;

    /// <summary>
    /// Default constructor
    /// </summary>
    public PagedResult()
    {
    }

    /// <summary>
    /// Constructor with parameters
    /// </summary>
    /// <param name="items">Items in the current page</param>
    /// <param name="pageIndex">Current page number (1-based)</param>
    /// <param name="pageSize">Size of a page</param>
    /// <param name="totalCount">Total count of items across all pages</param>
    public PagedResult(IEnumerable<T> items, int pageIndex, int pageSize, int totalCount)
    {
        Items = items ?? [];
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0;
    }
}
