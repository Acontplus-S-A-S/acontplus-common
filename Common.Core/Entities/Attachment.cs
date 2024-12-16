using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Core.Entities;
[Table("Attachment", Schema = "Common")]
public class Attachment
{
    public int NotificationId { get; set; }
    public Notification Notification { get; set; }
    [Required, MaxLength(300)] public string FileName { get; set; } 
    [Required, MaxLength(50)] public string FileType { get; set; }
    public int? FileSize { get; set; }
    [MaxLength(300)] public string FilePath { get; set; } // Nullable for optional local storage
    [MaxLength(300)] public string S3ObjectKey { get; set; } // Nullable for optional cloud storage
    [MaxLength(300)] public string S3ObjectUrl { get; set; } // Nullable for optional cloud URL
}
