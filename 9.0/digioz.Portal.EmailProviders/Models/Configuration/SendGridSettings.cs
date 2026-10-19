namespace digioz.Portal.EmailProviders.Models.Configuration;

/// <summary>
/// SendGrid email provider settings
/// </summary>
public class SendGridSettings
{
    /// <summary>
    /// SendGrid API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Optional: SendGrid template ID for templated emails
    /// </summary>
    public string? TemplateId { get; set; }

    /// <summary>
    /// Enable click tracking
    /// </summary>
    public bool EnableClickTracking { get; set; } = true;

    /// <summary>
    /// Enable open tracking
    /// </summary>
    public bool EnableOpenTracking { get; set; } = true;

    /// <summary>
    /// Sandbox mode (for testing without sending actual emails)
    /// </summary>
    public bool SandboxMode { get; set; } = false;
}
