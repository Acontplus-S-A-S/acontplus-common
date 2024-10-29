using Common.Core.Utils;
using Microsoft.AspNetCore.Http;

namespace Common.Core.Models;

public class FileModel : IDisposable
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public byte[] Content { get; private set; }
    public string Base64 { get; private set; }

    public void Dispose()
    {
        DisposeContent(); // Clear the content byte array
        Base64 = null; // Set Base64 to null to release memory
    }

    public void CreateBase64(byte[] content, string contentType, string fileName = null)
    {
        if (Content != null)
        {
            // Release previous content
            Array.Clear(Content, 0, Content.Length);
            Content = null;
        }

        Content = content;
        ContentType = contentType;
        FileName = fileName;

        // Convert to Base64 and immediately release the content if needed
        Base64 = Convert.ToBase64String(content, 0, content.Length);
    }

    public void CreateBytesCompressedDeflated(IFormFile file)
    {
        DisposeContent(); // Clear any existing content
        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);
            Content = CompressionUtils.CompressDeflate(memoryStream.ToArray());
        }

        ContentType = file.ContentType;
        FileName = FileExtensions.SanitizeFileName(file.FileName);
    }

    public void CreateBytesCompressedGzip(IFormFile file)
    {
        DisposeContent(); // Clear any existing content
        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);
            Content = CompressionUtils.CompressGZip(memoryStream.ToArray());
        }

        ContentType = file.ContentType;
        FileName = FileExtensions.SanitizeFileName(file.FileName);
    }

    private void DisposeContent()
    {
        if (Content != null)
        {
            // Ensure memory is released by clearing the byte array
            Array.Clear(Content, 0, Content.Length);
            Content = null;
        }
    }
}
