namespace digioz.Portal.EmailProviders.Models.Configuration;

/// <summary>
/// Main configuration class for email providers
/// Maps to ConfigKey/ConfigValue pairs from your Config table
/// </summary>
public class EmailConfiguration
{
    /// <summary>
    /// Provider type: SMTP, SendGrid, Mailgun, AzureEmail
    /// </summary>
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>
    /// Default from email address
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Default from display name
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Enable/disable email sending globally
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Provider-specific settings (SMTP, SendGrid, etc.)
    /// </summary>
    public SmtpSettings? SmtpSettings { get; set; }
    public SendGridSettings? SendGridSettings { get; set; }
    public MailgunSettings? MailgunSettings { get; set; }
    public AzureEmailSettings? AzureEmailSettings { get; set; }

    /// <summary>
    /// Optional: Override timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
