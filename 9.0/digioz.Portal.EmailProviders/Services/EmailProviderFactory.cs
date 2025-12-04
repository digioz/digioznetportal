using digioz.Portal.EmailProviders.Exceptions;
using digioz.Portal.EmailProviders.Interfaces;
using digioz.Portal.EmailProviders.Models.Configuration;
using digioz.Portal.EmailProviders.Providers;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.EmailProviders.Services;

/// <summary>
/// Factory for creating email provider instances based on configuration
/// </summary>
public class EmailProviderFactory : IEmailProviderFactory
{
    private readonly ILogger<EmailProviderFactory> _logger;

    public EmailProviderFactory(ILogger<EmailProviderFactory> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates an email provider instance based on the configuration
    /// </summary>
    public IEmailProvider CreateProvider(EmailConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (string.IsNullOrWhiteSpace(configuration.ProviderType))
        {
            throw new EmailConfigurationException("ProviderType is required in EmailConfiguration");
        }

        _logger.LogDebug("Creating email provider for type: {ProviderType}", configuration.ProviderType);

        return configuration.ProviderType.ToUpperInvariant() switch
        {
            "SMTP" => CreateSmtpProvider(configuration),
            "SENDGRID" => CreateSendGridProvider(configuration),
            "MAILGUN" => CreateMailgunProvider(configuration),
            "AZUREEMAIL" or "AZURE" => CreateAzureEmailProvider(configuration),
            _ => throw new EmailConfigurationException($"Unsupported provider type: {configuration.ProviderType}")
        };
    }

    /// <summary>
    /// Gets a list of all supported provider types
    /// </summary>
    public IEnumerable<string> GetSupportedProviders()
    {
        return new[] { "SMTP", "SendGrid", "Mailgun", "AzureEmail" };
    }

    private IEmailProvider CreateSmtpProvider(EmailConfiguration configuration)
    {
        if (configuration.SmtpSettings == null)
        {
            throw new EmailConfigurationException("SmtpSettings is required when ProviderType is SMTP", "SMTP");
        }

        return new SmtpEmailProvider(configuration.SmtpSettings, configuration.FromEmail, configuration.FromName, _logger);
    }

    private IEmailProvider CreateSendGridProvider(EmailConfiguration configuration)
    {
        if (configuration.SendGridSettings == null)
        {
            throw new EmailConfigurationException("SendGridSettings is required when ProviderType is SendGrid", "SendGrid");
        }

        return new SendGridEmailProvider(configuration.SendGridSettings, configuration.FromEmail, configuration.FromName, _logger);
    }

    private IEmailProvider CreateMailgunProvider(EmailConfiguration configuration)
    {
        if (configuration.MailgunSettings == null)
        {
            throw new EmailConfigurationException("MailgunSettings is required when ProviderType is Mailgun", "Mailgun");
        }

        return new MailgunEmailProvider(configuration.MailgunSettings, configuration.FromEmail, configuration.FromName, _logger);
    }

    private IEmailProvider CreateAzureEmailProvider(EmailConfiguration configuration)
    {
        if (configuration.AzureEmailSettings == null)
        {
            throw new EmailConfigurationException("AzureEmailSettings is required when ProviderType is AzureEmail", "AzureEmail");
        }

        return new AzureEmailProvider(configuration.AzureEmailSettings, configuration.FromEmail, configuration.FromName, _logger);
    }
}
