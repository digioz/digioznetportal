using digioz.Portal.EmailProviders.Models;

namespace digioz.Portal.EmailProviders.Interfaces;

/// <summary>
/// Interface for specific email provider implementations
/// </summary>
public interface IEmailProvider
{
    /// <summary>
    /// Gets the provider name (SMTP, SendGrid, etc.)
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Sends an email using the specific provider
    /// </summary>
    Task<EmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates provider-specific configuration
    /// </summary>
    Task<bool> ValidateAsync(CancellationToken cancellationToken = default);
}
