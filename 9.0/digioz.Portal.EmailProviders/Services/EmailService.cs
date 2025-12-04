using digioz.Portal.EmailProviders.Exceptions;
using digioz.Portal.EmailProviders.Interfaces;
using digioz.Portal.EmailProviders.Models;
using digioz.Portal.EmailProviders.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.EmailProviders.Services;

/// <summary>
/// Main email service implementation that orchestrates email sending through various providers
/// </summary>
public class EmailService : IEmailService
{
    private readonly IEmailProviderFactory _providerFactory;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IEmailProviderFactory providerFactory, ILogger<EmailService> logger)
    {
        _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends an email using the configured provider
    /// </summary>
    public async Task<EmailResult> SendEmailAsync(EmailMessage message, EmailConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        // Check if email sending is enabled
        if (!configuration.IsEnabled)
        {
            _logger.LogWarning("Email sending is disabled in configuration");
            return EmailResult.Failure("Email sending is disabled in configuration", configuration.ProviderType);
        }

        // Validate the message
        if (!message.IsValid(out var errors))
        {
            var errorMessage = $"Invalid email message: {string.Join(", ", errors)}";
            _logger.LogError(errorMessage);
            return EmailResult.Failure(errorMessage, configuration.ProviderType);
        }

        // Apply default From address if not specified
        if (message.From == null || string.IsNullOrWhiteSpace(message.From.Email))
        {
            message.From = new EmailAddress(configuration.FromEmail, configuration.FromName);
        }

        try
        {
            _logger.LogInformation("Sending email via {ProviderType} to {Recipient}", 
                configuration.ProviderType, 
                message.To.Email);

            var provider = _providerFactory.CreateProvider(configuration);
            var result = await provider.SendAsync(message, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Email sent successfully via {ProviderName}. MessageId: {MessageId}", 
                    result.ProviderName, 
                    result.MessageId);
            }
            else
            {
                _logger.LogError("Failed to send email via {ProviderName}. Error: {ErrorMessage}", 
                    result.ProviderName, 
                    result.ErrorMessage);
            }

            return result;
        }
        catch (EmailProviderException ex)
        {
            _logger.LogError(ex, "Email provider exception occurred: {Message}", ex.Message);
            return EmailResult.Failure(ex.Message, configuration.ProviderType, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email: {Message}", ex.Message);
            return EmailResult.Failure($"Unexpected error: {ex.Message}", configuration.ProviderType, ex);
        }
    }

    /// <summary>
    /// Sends an email with retry logic
    /// </summary>
    public async Task<EmailResult> SendEmailWithRetryAsync(EmailMessage message, EmailConfiguration configuration, int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        if (maxRetries < 1)
        {
            throw new ArgumentException("maxRetries must be at least 1", nameof(maxRetries));
        }

        EmailResult? lastResult = null;
        var attempt = 0;

        while (attempt < maxRetries)
        {
            attempt++;
            _logger.LogDebug("Email send attempt {Attempt} of {MaxRetries}", attempt, maxRetries);

            lastResult = await SendEmailAsync(message, configuration, cancellationToken);

            if (lastResult.IsSuccess)
            {
                return lastResult;
            }

            if (attempt < maxRetries)
            {
                var delaySeconds = Math.Pow(2, attempt); // Exponential backoff: 2, 4, 8 seconds
                _logger.LogWarning("Email send failed. Retrying in {DelaySeconds} seconds...", delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }
        }

        _logger.LogError("Email sending failed after {MaxRetries} attempts", maxRetries);
        return lastResult ?? EmailResult.Failure("All retry attempts failed", configuration.ProviderType);
    }

    /// <summary>
    /// Validates email configuration without sending
    /// </summary>
    public async Task<bool> ValidateConfigurationAsync(EmailConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        try
        {
            _logger.LogDebug("Validating configuration for provider: {ProviderType}", configuration.ProviderType);

            var provider = _providerFactory.CreateProvider(configuration);
            var isValid = await provider.ValidateAsync(cancellationToken);

            _logger.LogInformation("Configuration validation result for {ProviderType}: {IsValid}", 
                configuration.ProviderType, 
                isValid);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration validation failed: {Message}", ex.Message);
            return false;
        }
    }
}
