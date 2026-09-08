namespace digioz.Portal.EmailProviders.Models;

/// <summary>
/// Represents an email attachment
/// </summary>
public class EmailAttachment
{
    /// <summary>
    /// File name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File content as byte array
    /// </summary>
    public byte[] Content { get; set; } = [];

    /// <summary>
    /// MIME content type (e.g., application/pdf, image/jpeg)
    /// </summary>
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>
    /// Optional: Content ID for inline images
    /// </summary>
    public string? ContentId { get; set; }

    public EmailAttachment()
    {
    }

    public EmailAttachment(string fileName, byte[] content, string contentType = "application/octet-stream")
    {
        FileName = fileName;
        Content = content;
        ContentType = contentType;
    }
}
