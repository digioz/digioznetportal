using System.Net;
using System.Net.Mail;
using digioz.Portal.EmailProviders.Exceptions;
using digioz.Portal.EmailProviders.Interfaces;
using digioz.Portal.EmailProviders.Models;
using digioz.Portal.EmailProviders.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.EmailProviders.Providers;

/// <summary>
/// SMTP email provider implementation
/// </summary>
public class SmtpEmailProvider : IEmailProvider
{
    private readonly SmtpSettings _settings;
    private readonly string _defaultFromEmail;
    private readonly string _defaultFromName;
    private readonly ILogger _logger;

    public string ProviderName => "SMTP";

    public SmtpEmailProvider(SmtpSettings settings, string defaultFromEmail, string defaultFromName, ILogger logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _defaultFromEmail = defaultFromEmail;
        _defaultFromName = defaultFromName;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateSettings();
    }

    /// <summary>
    /// Sends an email using SMTP
    /// </summary>
    public async Task<EmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            using var smtpClient = CreateSmtpClient();
            using var mailMessage = CreateMailMessage(message);

            _logger.LogDebug("Sending email via SMTP to {Host}:{Port}", _settings.Host, _settings.Port);

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            var messageId = mailMessage.Headers["Message-ID"] ?? Guid.NewGuid().ToString();
            _logger.LogInformation("Email sent successfully via SMTP. MessageId: {MessageId}", messageId);

            return EmailResult.Success(messageId, ProviderName);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error occurred: {Message}", ex.Message);
            return EmailResult.Failure($"SMTP error: {ex.Message}", ProviderName, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email via SMTP: {Message}", ex.Message);
            return EmailResult.Failure($"Error: {ex.Message}", ProviderName, ex);
        }
    }

    /// <summary>
    /// Validates SMTP configuration
    /// </summary>
    public async Task<bool> ValidateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var smtpClient = CreateSmtpClient();
            
            // Note: SmtpClient doesn't have a native test connection method
            // We just verify that we can create the client with the settings
            _logger.LogInformation("SMTP configuration validated successfully");
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP configuration validation failed: {Message}", ex.Message);
            return false;
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            UseDefaultCredentials = _settings.UseDefaultCredentials,
            Timeout = _settings.Timeout
        };

        if (!_settings.UseDefaultCredentials && !string.IsNullOrWhiteSpace(_settings.Username))
        {
            smtpClient.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
        }

        return smtpClient;
    }

    private MailMessage CreateMailMessage(EmailMessage message)
    {
        var mailMessage = new MailMessage
        {
            Subject = message.Subject,
            IsBodyHtml = !string.IsNullOrWhiteSpace(message.HtmlBody)
        };

        // Set body (prefer HTML over text)
        if (!string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            mailMessage.Body = message.HtmlBody;
            
            // If we also have a text body, add it as an alternative view
            if (!string.IsNullOrWhiteSpace(message.TextBody))
            {
                var plainView = AlternateView.CreateAlternateViewFromString(message.TextBody, null, "text/plain");
                mailMessage.AlternateViews.Add(plainView);
            }
        }
        else if (!string.IsNullOrWhiteSpace(message.TextBody))
        {
            mailMessage.Body = message.TextBody;
        }

        // Set priority
        mailMessage.Priority = message.Priority switch
        {
            EmailPriority.High => MailPriority.High,
            EmailPriority.Low => MailPriority.Low,
            _ => MailPriority.Normal
        };

        // Set From address
        if (message.From != null && !string.IsNullOrWhiteSpace(message.From.Email))
        {
            mailMessage.From = new MailAddress(message.From.Email, message.From.Name ?? string.Empty);
        }
        else
        {
            mailMessage.From = new MailAddress(_defaultFromEmail, _defaultFromName);
        }

        // Set ReplyTo
        if (message.ReplyTo != null && !string.IsNullOrWhiteSpace(message.ReplyTo.Email))
        {
            mailMessage.ReplyToList.Add(new MailAddress(message.ReplyTo.Email, message.ReplyTo.Name ?? string.Empty));
        }

        // Add To recipients
        if (!string.IsNullOrWhiteSpace(message.To.Email))
        {
            mailMessage.To.Add(new MailAddress(message.To.Email, message.To.Name ?? string.Empty));
        }

        foreach (var toAddress in message.ToAddresses.Where(a => !string.IsNullOrWhiteSpace(a.Email)))
        {
            mailMessage.To.Add(new MailAddress(toAddress.Email, toAddress.Name ?? string.Empty));
        }

        // Add CC recipients
        foreach (var ccAddress in message.CcAddresses.Where(a => !string.IsNullOrWhiteSpace(a.Email)))
        {
            mailMessage.CC.Add(new MailAddress(ccAddress.Email, ccAddress.Name ?? string.Empty));
        }

        // Add BCC recipients
        foreach (var bccAddress in message.BccAddresses.Where(a => !string.IsNullOrWhiteSpace(a.Email)))
        {
            mailMessage.Bcc.Add(new MailAddress(bccAddress.Email, bccAddress.Name ?? string.Empty));
        }

        // Add attachments
        foreach (var attachment in message.Attachments)
        {
            var stream = new MemoryStream(attachment.Content);
            var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
            
            if (!string.IsNullOrWhiteSpace(attachment.ContentId))
            {
                mailAttachment.ContentId = attachment.ContentId;
            }
            
            mailMessage.Attachments.Add(mailAttachment);
        }

        // Add custom headers
        foreach (var header in message.CustomHeaders)
        {
            mailMessage.Headers.Add(header.Key, header.Value);
        }

        // Generate and add Message-ID header (RFC 5322 compliant)
        // Format: <unique-id@domain>
        var fromDomain = mailMessage.From.Host;
        var messageId = $"<{Guid.NewGuid():N}.{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}@{fromDomain}>";
        mailMessage.Headers.Add("Message-ID", messageId);

        return mailMessage;
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            throw new EmailConfigurationException("SMTP Host is required", ProviderName);
        }

        if (_settings.Port <= 0 || _settings.Port > 65535)
        {
            throw new EmailConfigurationException("SMTP Port must be between 1 and 65535", ProviderName);
        }

        if (!_settings.UseDefaultCredentials && string.IsNullOrWhiteSpace(_settings.Username))
        {
            throw new EmailConfigurationException("SMTP Username is required when not using default credentials", ProviderName);
        }
    }
}
