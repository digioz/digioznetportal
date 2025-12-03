namespace digioz.Portal.EmailProviders.Exceptions;

/// <summary>
/// Exception thrown by email providers
/// </summary>
public class EmailProviderException : Exception
{
    /// <summary>
    /// Name of the provider that threw the exception
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// Additional error details
    /// </summary>
    public Dictionary<string, string> Details { get; set; } = [];

    public EmailProviderException()
    {
    }

    public EmailProviderException(string message) : base(message)
    {
    }

    public EmailProviderException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public EmailProviderException(string message, string providerName) : base(message)
    {
        ProviderName = providerName;
    }

    public EmailProviderException(string message, string providerName, Exception innerException) : base(message, innerException)
    {
        ProviderName = providerName;
    }
}
