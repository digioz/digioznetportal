using Azure;
using Azure.Communication.Email;
using digioz.Portal.EmailProviders.Exceptions;
using digioz.Portal.EmailProviders.Interfaces;
using digioz.Portal.EmailProviders.Models.Configuration;
using Microsoft.Extensions.Logging;
using AzureEmailAddress = Azure.Communication.Email.EmailAddress;
using AzureEmailAttachment = Azure.Communication.Email.EmailAttachment;
using AzureEmailMessage = Azure.Communication.Email.EmailMessage;
using PortalEmailMessage = digioz.Portal.EmailProviders.Models.EmailMessage;

namespace digioz.Portal.EmailProviders.Providers;

/// <summary>
/// Azure Communication Services Email provider implementation
/// </summary>
public class AzureEmailProvider : IEmailProvider
{
    private readonly AzureEmailSettings _settings;
    private readonly string _defaultFromEmail;
    private readonly string _defaultFromName;
    private readonly ILogger _logger;
    private readonly EmailClient _emailClient;

    public string ProviderName => "AzureEmail";

    public AzureEmailProvider(AzureEmailSettings settings, string defaultFromEmail, string defaultFromName, ILogger logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _defaultFromEmail = defaultFromEmail;
        _defaultFromName = defaultFromName;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateSettings();

        // Create EmailClient
        _emailClient = new EmailClient(_settings.ConnectionString);
    }

    /// <summary>
    /// Sends an email using Azure Communication Services Email API
    /// </summary>
    public async Task<Models.EmailResult> SendAsync(PortalEmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var azureEmailMessage = CreateAzureEmailMessage(message);

            _logger.LogDebug("Sending email via Azure Communication Services");

            // Send the email and wait for completion
            EmailSendOperation emailSendOperation = await _emailClient.SendAsync(
                WaitUntil.Completed,
                azureEmailMessage,
                cancellationToken
            );

            if (emailSendOperation.HasCompleted)
            {
                var messageId = emailSendOperation.Id;
                
                // Check if the operation was successful
                if (emailSendOperation.HasValue)
                {
                    var status = emailSendOperation.Value.Status;
                    
                    if (status == EmailSendStatus.Succeeded)
                    {
                        _logger.LogInformation("Email sent successfully via Azure Email. MessageId: {MessageId}", messageId);
                        return Models.EmailResult.Success(messageId, ProviderName);
                    }
                    else
                    {
                        var errorMessage = $"Azure Email send status: {status}";
                        _logger.LogError(errorMessage);
                        return Models.EmailResult.Failure(errorMessage, ProviderName);
                    }
                }
                else
                {
                    _logger.LogInformation("Email sent via Azure Email. MessageId: {MessageId}", messageId);
                    return Models.EmailResult.Success(messageId, ProviderName);
                }
            }
            else
            {
                var errorMessage = "Azure Email send operation did not complete";
                _logger.LogError(errorMessage);
                return Models.EmailResult.Failure(errorMessage, ProviderName);
            }
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Email request failed: {Message}, Status: {Status}", ex.Message, ex.Status);
            return Models.EmailResult.Failure($"Azure Email request failed: {ex.Message} (Status: {ex.Status})", ProviderName, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via Azure Email: {Message}", ex.Message);
            return Models.EmailResult.Failure($"Azure Email error: {ex.Message}", ProviderName, ex);
        }
    }

    /// <summary>
    /// Validates Azure Email configuration
    /// </summary>
    public async Task<bool> ValidateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.ConnectionString))
            {
                _logger.LogError("Azure Email connection string is missing");
                return false;
            }

            // Test the connection by creating a client
            // Azure doesn't provide a specific validation endpoint, so we just verify client creation
            var testClient = new EmailClient(_settings.ConnectionString);
            
            _logger.LogInformation("Azure Email configuration validated successfully");
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Email configuration validation failed: {Message}", ex.Message);
            return false;
        }
    }

    private AzureEmailMessage CreateAzureEmailMessage(PortalEmailMessage message)
    {
        // Create email content
        var emailContent = new EmailContent(message.Subject);

        if (!string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            emailContent.Html = message.HtmlBody;
        }

        if (!string.IsNullOrWhiteSpace(message.TextBody))
        {
            emailContent.PlainText = message.TextBody;
        }

        // Set sender address
        string senderAddress;
        if (message.From != null && !string.IsNullOrWhiteSpace(message.From.Email))
        {
            senderAddress = message.From.Email;
        }
        else
        {
            senderAddress = _defaultFromEmail;
        }

        // Create recipients
        var recipients = new EmailRecipients();

        // Add To recipients
        if (!string.IsNullOrWhiteSpace(message.To.Email))
        {
            recipients.To.Add(new AzureEmailAddress(message.To.Email, message.To.Name));
        }

        foreach (var toAddress in message.ToAddresses.Where(a => !string.IsNullOrWhiteSpace(a.Email)))
        {
            recipients.To.Add(new AzureEmailAddress(toAddress.Email, toAddress.Name));
        }

        // Add CC recipients
        foreach (var ccAddress in message.CcAddresses.Where(a => !string.IsNullOrWhiteSpace(a.Email)))
        {
            recipients.CC.Add(new AzureEmailAddress(ccAddress.Email, ccAddress.Name));
        }

        // Add BCC recipients
        foreach (var bccAddress in message.BccAddresses.Where(a => !string.IsNullOrWhiteSpace(a.Email)))
        {
            recipients.BCC.Add(new AzureEmailAddress(bccAddress.Email, bccAddress.Name));
        }

        // Create the email message
        var azureEmailMessage = new AzureEmailMessage(senderAddress, recipients, emailContent);

        // Set ReplyTo
        if (message.ReplyTo != null && !string.IsNullOrWhiteSpace(message.ReplyTo.Email))
        {
            azureEmailMessage.ReplyTo.Add(new AzureEmailAddress(message.ReplyTo.Email, message.ReplyTo.Name));
        }

        // Add attachments
        foreach (var attachment in message.Attachments)
        {
            var azureAttachment = new AzureEmailAttachment(
                attachment.FileName,
                attachment.ContentType,
                BinaryData.FromBytes(attachment.Content)
            );

            if (!string.IsNullOrWhiteSpace(attachment.ContentId))
            {
                azureAttachment.ContentId = attachment.ContentId;
            }

            azureEmailMessage.Attachments.Add(azureAttachment);
        }

        // Add custom headers
        foreach (var header in message.CustomHeaders)
        {
            azureEmailMessage.Headers.Add(header.Key, header.Value);
        }

        // Note: Azure Communication Services Email API doesn't support Importance property in current SDK version
        // Priority can be set via custom headers if needed
        if (message.Priority == Models.EmailPriority.High)
        {
            azureEmailMessage.Headers.Add("X-Priority", "1");
            azureEmailMessage.Headers.Add("Importance", "high");
        }
        else if (message.Priority == Models.EmailPriority.Low)
        {
            azureEmailMessage.Headers.Add("X-Priority", "5");
            azureEmailMessage.Headers.Add("Importance", "low");
        }

        // Enable user engagement tracking if configured
        if (_settings.EnableTracking)
        {
            azureEmailMessage.UserEngagementTrackingDisabled = false;
        }
        else
        {
            azureEmailMessage.UserEngagementTrackingDisabled = true;
        }

        return azureEmailMessage;
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.ConnectionString))
        {
            throw new EmailConfigurationException("Azure Email Connection String is required", ProviderName);
        }
    }
}
