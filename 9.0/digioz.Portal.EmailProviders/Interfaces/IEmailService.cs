using digioz.Portal.EmailProviders.Models;
using digioz.Portal.EmailProviders.Models.Configuration;

namespace digioz.Portal.EmailProviders.Interfaces;

/// <summary>
/// Main email service interface for sending emails through various providers
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email using the configured provider
    /// </summary>
    Task<EmailResult> SendEmailAsync(EmailMessage message, EmailConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email with retry logic
    /// </summary>
    Task<EmailResult> SendEmailWithRetryAsync(EmailMessage message, EmailConfiguration configuration, int maxRetries = 3, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates email configuration without sending
    /// </summary>
    Task<bool> ValidateConfigurationAsync(EmailConfiguration configuration, CancellationToken cancellationToken = default);
}
