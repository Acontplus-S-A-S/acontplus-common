namespace Common.Core.DTOs;

public class BaseDto
{
    public int? UserId { get; set; }
    public int? UserRoleId { get; set; }
    public bool? Enabled { get; set; } = true; // Deprecated, use IsActive instead
    public bool? IsActive { get; set; } = true;
    public bool? IsDeleted { get; set; }
    public bool? FromMobile { get; set; }
}

