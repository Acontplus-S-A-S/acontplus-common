namespace Common.Notifications.Entities;
[Table("Notification", Schema = "Common")]
public class NotificationType : BaseEntity
{
    [Required, MaxLength(5)] public string Code { get; set; }
    public string Name { get; set; } // e.g., "Email", "Sms", "Push", "WhatsApp"
}
