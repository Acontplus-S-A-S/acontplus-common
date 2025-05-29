namespace Common.Core.Models;

/// <summary>
/// Generic class for paginated results
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PagedResult<T> where T : class
{
    /// <summary>
    /// Items in the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

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
}
