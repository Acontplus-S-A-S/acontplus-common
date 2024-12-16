using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Common.Core.Utils;

namespace Common.Core.Entities;

[Table("Notification", Schema = "Common")]
public class Notification : BaseEntity
{
    private string _decompressedContent;
    private string _decompressedParameters;
    [Required] public byte[] CompressedParameters { get; set; }
    [Required] public byte[] CompressedContent { get; set; }
    [Required] public string RecipientEmail { get; set; }
    [Required] public string Status { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public string ErrorMessage { get; set; }
    public ICollection<Attachment> Attachments { get; set; }

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
    [NotMapped]
    public string Parameters
    {
        get
        {
            switch (_decompressedParameters)
            {
                case null when CompressedParameters != null:
                {
                    var decompressedBytes = CompressionUtils.DecompressGZip(CompressedParameters);
                        _decompressedParameters = Encoding.UTF8.GetString(decompressedBytes);
                    break;
                }
            }

            return _decompressedParameters;
        }
        set
        {
            var stringBytes = Encoding.UTF8.GetBytes(value);
            CompressedParameters = CompressionUtils.CompressGZip(stringBytes);
            _decompressedParameters = value; // Cache the value
        }
    }
}
