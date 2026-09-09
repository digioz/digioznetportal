namespace digioz.Portal.EmailProviders.Models.Configuration;

/// <summary>
/// Mailgun email provider settings
/// </summary>
public class MailgunSettings
{
    /// <summary>
    /// Mailgun API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Mailgun domain
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Mailgun API base URL (US or EU region)
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.mailgun.net/v3";

    /// <summary>
    /// Enable tracking
    /// </summary>
    public bool EnableTracking { get; set; } = true;

    /// <summary>
    /// Enable DKIM signatures
    /// </summary>
    public bool EnableDkim { get; set; } = true;
}
