namespace Common.Core.DTOs;

public class PaginationDto
{
    // Basic pagination
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // User context
    public int? UserId { get; set; }

    // Search and filtering
    public string TextSearch { get; set; }
    public bool? IsEnabled { get; set; }

    // Sorting
    public string SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}
