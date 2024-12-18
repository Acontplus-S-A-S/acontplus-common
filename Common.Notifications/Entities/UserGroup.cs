namespace Common.Notifications.Entities;
[Table("UserGroup", Schema = "Common")]
public class UserGroup : BaseEntity
{
    public int GroupId { get; set; }
    public NotificationGroup Group { get; set; }
}
