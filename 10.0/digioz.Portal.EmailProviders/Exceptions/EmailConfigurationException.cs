namespace digioz.Portal.EmailProviders.Exceptions;

/// <summary>
/// Exception thrown when email configuration is invalid
/// </summary>
public class EmailConfigurationException : EmailProviderException
{
    public EmailConfigurationException()
    {
    }

    public EmailConfigurationException(string message) : base(message)
    {
    }

    public EmailConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public EmailConfigurationException(string message, string providerName) : base(message, providerName)
    {
    }
}
