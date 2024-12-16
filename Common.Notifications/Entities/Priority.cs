namespace Common.Notifications.Entities;
[Table("Priority", Schema = "Common")]
public class Priority : BaseEntity
{
    [Required, MaxLength(5)] public string Code { get; set; }
    public string Name { get; set; } // e.g., "High", "Medium", "Low"
}
