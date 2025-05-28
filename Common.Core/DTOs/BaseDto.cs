namespace Common.Core.DTOs;

public class BaseDto
{
    public int? CreatedByUserId { get; set; }
    public int? UpdatedByUserId { get; set; }
    public int? DeletedByUserId { get; set; }
    public int? UserRoleId { get; set; }
    public bool? IsActive { get; set; } = true;
    public bool? IsDeleted { get; set; }
    public bool? FromMobile { get; set; }
}

