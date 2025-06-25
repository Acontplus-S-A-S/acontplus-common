namespace Common.Core.DTOs;

/// <summary>
/// DTO for pagination parameters
/// </summary>
public class PaginationDto
{
    private int _pageIndex = 1;
    private int _pageSize = 10;

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : value > 100 ? 100 : value;
    }

    /// <summary>
    /// Property name to sort by
    /// </summary>
    public string SortBy { get; set; }

    /// <summary>
    /// Sort direction ("asc" or "desc")
    /// </summary>
    public string SortDirection { get; set; } = "asc";

    /// <summary>
    /// Optional search term
    /// </summary>
    public string SearchTerm { get; set; }

    /// <summary>
    /// Optional filter criteria as a dictionary
    /// </summary>
    public Dictionary<string, string> Filters { get; set; }
}
