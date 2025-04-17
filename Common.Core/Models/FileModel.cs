using Common.Core.Format;
using Common.Core.IO;
using Microsoft.AspNetCore.Http;

namespace Common.Core.Models;

/// <summary>
/// Represents a file with content, metadata, and compression capabilities.
/// </summary>
public class FileModel : IDisposable
{
    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the MIME content type of the file.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the binary content of the file.
    /// </summary>
    public byte[] Content { get; set; }

    /// <summary>
    /// Gets or sets the Base64 string representation of the content.
    /// </summary>
    public string Base64 { get; set; }

    /// <summary>
    /// Flag to track whether the object has been disposed.
    /// </summary>
    private bool _disposed = false;

    /// <summary>
    /// Creates a new file model with the specified content and metadata.
    /// </summary>
    /// <param name="content">The binary content of the file.</param>
    /// <param name="contentType">The MIME content type.</param>
    /// <param name="fileName">The name of the file (optional).</param>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public void Create(byte[] content, string contentType, string fileName = null)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        DisposeContent();

        Content = content;
        ContentType = contentType ?? "application/octet-stream";
        FileName = fileName;
    }

    /// <summary>
    /// Creates a file model with Base64 encoded content.
    /// </summary>
    /// <param name="content">The binary content to encode as Base64.</param>
    /// <param name="contentType">The MIME content type.</param>
    /// <param name="fileName">The name of the file (optional).</param>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public void CreateBase64(byte[] content, string contentType, string fileName = null)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        DisposeContent();

        ContentType = contentType ?? "application/octet-stream";
        FileName = fileName;
        Base64 = Convert.ToBase64String(content);
    }

    /// <summary>
    /// Creates a file model with Deflate-compressed content from an IFormFile.
    /// </summary>
    /// <param name="file">The form file to compress.</param>
    /// <exception cref="ArgumentNullException">Thrown when file is null.</exception>
    public async Task CreateBytesCompressedDeflatedAsync(IFormFile file)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));

        await CompressFileAsync(file, CompressionUtils.CompressDeflate);
    }

    /// <summary>
    /// Creates a file model with GZip-compressed content from an IFormFile.
    /// </summary>
    /// <param name="file">The form file to compress.</param>
    /// <exception cref="ArgumentNullException">Thrown when file is null.</exception>
    public async Task CreateBytesCompressedGzipAsync(IFormFile file)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));

        await CompressFileAsync(file, CompressionUtils.CompressGZip);
    }

    /// <summary>
    /// Helper method to compress a file using the specified compression function.
    /// </summary>
    private async Task CompressFileAsync(IFormFile file, Func<byte[], byte[]> compressionFunc)
    {
        DisposeContent();

        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            Content = compressionFunc(memoryStream.ToArray());
        }

        ContentType = file.ContentType;
        FileName = FileExtensions.SanitizeFileName(file.FileName);
    }

    /// <summary>
    /// Clears the content byte array to release memory.
    /// </summary>
    private void DisposeContent()
    {
        if (Content != null)
        {
            Array.Clear(Content, 0, Content.Length);
            Content = null;
        }

        Base64 = null;
    }

    /// <summary>
    /// Disposes resources used by the FileModel.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources used by the FileModel.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DisposeContent();
            }

            _disposed = true;
        }
    }
}
