using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Common.Core.Utils;

namespace Common.Core.Entities;

[Table("Notification", Schema = "Common")]
public class Notification : BaseEntity
{
    private string _decompressedContent;
    [Required] public string Parameters { get; set; }
    [Required] private byte[] CompressedContent { get; set; }
    [Required] public string Receiver { get; set; }
    [Required] public string Status { get; set; }
    public DateTime? SentDate { get; set; }
    public string Error { get; set; }

    [NotMapped]
    public string Content
    {
        get
        {
            switch (_decompressedContent)
            {
                case null when CompressedContent != null:
                {
                    var decompressedBytes = CompressionUtils.DecompressGZip(CompressedContent);
                    _decompressedContent = Encoding.UTF8.GetString(decompressedBytes);
                    break;
                }
            }

            return _decompressedContent;
        }
        set
        {
            var stringBytes = Encoding.UTF8.GetBytes(value);
            CompressedContent = CompressionUtils.CompressGZip(stringBytes);
            _decompressedContent = value; // Cache the value
        }
    }
}
