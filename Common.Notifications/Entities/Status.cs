namespace Common.Notifications.Entities;
[Table("Status", Schema = "Common")]
public class Status : BaseEntity
{
    [Required, MaxLength(5)] public string Code { get; set; }
    public string Name { get; set; } // e.g., "Queued", "Processing", "Sent", "Failed"
}
