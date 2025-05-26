using System.ComponentModel.DataAnnotations;

namespace Common.Core.Entities;

public class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }
    public bool Enabled { get; set; } = true; // Deprecated field, use IsActive instead
    public bool IsActive { get; set; } = true;
    public bool Deleted { get; set; } = false; // Deprecated field, use IsDeleted instead
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserId { get; set; }
    public int? UserId { get; set; } // Deprecated field, use CreatedByUserId instead
    public bool FromMobile { get; set; } = false;
    [Timestamp]
    public byte[] RowVersion { get; set; }
}
