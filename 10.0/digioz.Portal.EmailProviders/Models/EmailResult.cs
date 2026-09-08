namespace digioz.Portal.EmailProviders.Models;

/// <summary>
/// Result of an email send operation
/// </summary>
public class EmailResult
{
    /// <summary>
    /// Indicates if the email was sent successfully
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Unique message ID from the provider
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Exception that occurred (if any)
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Name of the provider that handled the email
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// Timestamp when the email was sent
    /// </summary>
    public DateTime SentAt { get; set; }

    /// <summary>
    /// Additional metadata from the provider
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

    /// <summary>
    /// Creates a success result
    /// </summary>
    public static EmailResult Success(string messageId, string providerName)
    {
        return new EmailResult
        {
            IsSuccess = true,
            MessageId = messageId,
            ProviderName = providerName,
            SentAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static EmailResult Failure(string errorMessage, string providerName, Exception? exception = null)
    {
        return new EmailResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ProviderName = providerName,
            Exception = exception,
            SentAt = DateTime.UtcNow
        };
    }
}
