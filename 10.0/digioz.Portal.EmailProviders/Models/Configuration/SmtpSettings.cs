namespace digioz.Portal.EmailProviders.Models.Configuration;

/// <summary>
/// SMTP email provider settings
/// </summary>
public class SmtpSettings
{
    /// <summary>
    /// SMTP server host (e.g., smtp.gmail.com)
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port (typically 25, 587, or 465)
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// SMTP username for authentication
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// SMTP password for authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Enable SSL/TLS encryption
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Use default credentials
    /// </summary>
    public bool UseDefaultCredentials { get; set; } = false;

    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    public int Timeout { get; set; } = 30000;
}
