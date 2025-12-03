using digioz.Portal.EmailProviders.Exceptions;
using digioz.Portal.EmailProviders.Interfaces;
using digioz.Portal.EmailProviders.Models;
using digioz.Portal.EmailProviders.Models.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using SendGridEmailAddress = SendGrid.Helpers.Mail.EmailAddress;

namespace digioz.Portal.EmailProviders.Providers;

/// <summary>
/// SendGrid email provider implementation
/// </summary>
public class SendGridEmailProvider : IEmailProvider
{
    private readonly SendGridSettings _settings;
    private readonly string _defaultFromEmail;
    private readonly string _defaultFromName;
    private readonly ILogger _logger;

    public string ProviderName => "SendGrid";

    public SendGridEmailProvider(SendGridSettings settings, string defaultFromEmail, string defaultFromName, ILogger logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _defaultFromEmail = defaultFromEmail;
        _defaultFromName = defaultFromName;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateSettings();
    }

    /// <summary>
    /// Sends an email using SendGrid API
    /// </summary>
    public async Task<EmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new SendGridClient(_settings.ApiKey);
            var sendGridMessage = CreateSendGridMessage(message);

            _logger.LogDebug("Sending email via SendGrid");

            var response = await client.SendEmailAsync(sendGridMessage, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                // Try to get message ID from response headers
                string? messageId = null;
                if (response.Headers != null && response.Headers.TryGetValues("X-Message-Id", out var messageIds))
                {
                    messageId = messageIds.FirstOrDefault();
                }
                messageId ??= Guid.NewGuid().ToString();

                _logger.LogInformation("Email sent successfully via SendGrid. MessageId: {MessageId}", messageId);
                return EmailResult.Success(messageId, ProviderName);
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                var errorMessage = $"SendGrid API error: {response.StatusCode} - {errorBody}";
                _logger.LogError(errorMessage);
                return EmailResult.Failure(errorMessage, ProviderName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via SendGrid: {Message}", ex.Message);
            return EmailResult.Failure($"SendGrid error: {ex.Message}", ProviderName, ex);
        }
    }

    /// <summary>
    /// Validates SendGrid configuration by attempting to create a client
    /// </summary>
    public async Task<bool> ValidateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                _logger.LogError("SendGrid API key is missing");
                return false;
            }

            // Create a client to verify the API key format is valid
            var client = new SendGridClient(_settings.ApiKey);
            
            _logger.LogInformation("SendGrid configuration validated successfully");
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendGrid configuration validation failed: {Message}", ex.Message);
            return false;
        }
    }

    private SendGridMessage CreateSendGridMessage(EmailMessage message)
    {
        var msg = new SendGridMessage();

        // Set From address
        if (message.From != null && !string.IsNullOrWhiteSpace(message.From.Email))
        {
            msg.SetFrom(new SendGridEmailAddress(message.From.Email, message.From.Name));
        }
        else
        {
            msg.SetFrom(new SendGridEmailAddress(_defaultFromEmail, _defaultFromName));
        }

        // Set Subject
        msg.SetSubject(message.Subject);

        // Add To recipients
        var toAddresses = new List<SendGridEmailAddress>();
        if (!string.IsNullOrWhiteSpace(message.To.Email))
        {
            toAddresses.Add(new SendGridEmailAddress(message.To.Email, message.To.Name));
        }
        foreach (var toAddress in message.ToAddresses.Where(a => !string.IsNullOrWhiteSpace(a.Email)))
        {
            toAddresses.Add(new SendGridEmailAddress(toAddress.Email, toAddress.Name));
        }
        if (toAddresses.Any())
        {
            msg.AddTos(toAddresses);
        }

        // Add CC recipients
        var ccAddresses = message.CcAddresses
            .Where(a => !string.IsNullOrWhiteSpace(a.Email))
            .Select(a => new SendGridEmailAddress(a.Email, a.Name))
            .ToList();
        if (ccAddresses.Any())
        {
            msg.AddCcs(ccAddresses);
        }

        // Add BCC recipients
        var bccAddresses = message.BccAddresses
            .Where(a => !string.IsNullOrWhiteSpace(a.Email))
            .Select(a => new SendGridEmailAddress(a.Email, a.Name))
            .ToList();
        if (bccAddresses.Any())
        {
            msg.AddBccs(bccAddresses);
        }

        // Set ReplyTo
        if (message.ReplyTo != null && !string.IsNullOrWhiteSpace(message.ReplyTo.Email))
        {
            msg.SetReplyTo(new SendGridEmailAddress(message.ReplyTo.Email, message.ReplyTo.Name));
        }

        // Set content (plain text and/or HTML)
        if (!string.IsNullOrWhiteSpace(message.TextBody))
        {
            msg.AddContent(MimeType.Text, message.TextBody);
        }
        if (!string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            msg.AddContent(MimeType.Html, message.HtmlBody);
        }

        // Add attachments
        foreach (var attachment in message.Attachments)
        {
            var base64Content = Convert.ToBase64String(attachment.Content);
            msg.AddAttachment(new Attachment
            {
                Content = base64Content,
                Filename = attachment.FileName,
                Type = attachment.ContentType,
                Disposition = "attachment",
                ContentId = attachment.ContentId
            });
        }

        // Add custom headers
        foreach (var header in message.CustomHeaders)
        {
            msg.AddHeader(header.Key, header.Value);
        }

        // Add categories/tags
        if (message.Tags.Any())
        {
            msg.AddCategories(message.Tags.Select(t => t.Value).ToList());
        }

        // Configure tracking settings
        msg.SetClickTracking(_settings.EnableClickTracking, _settings.EnableClickTracking);
        msg.SetOpenTracking(_settings.EnableOpenTracking);

        // Set sandbox mode if enabled
        if (_settings.SandboxMode)
        {
            msg.MailSettings = new MailSettings
            {
                SandboxMode = new SandboxMode { Enable = true }
            };
        }

        // Use template if specified
        if (!string.IsNullOrWhiteSpace(_settings.TemplateId))
        {
            msg.SetTemplateId(_settings.TemplateId);
            
            // If using a template, template data can be passed via custom data
            if (message.Tags.Any())
            {
                msg.SetTemplateData(message.Tags);
            }
        }

        return msg;
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new EmailConfigurationException("SendGrid API Key is required", ProviderName);
        }
    }
}
