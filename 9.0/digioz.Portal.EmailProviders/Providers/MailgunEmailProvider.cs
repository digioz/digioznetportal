using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using digioz.Portal.EmailProviders.Exceptions;
using digioz.Portal.EmailProviders.Interfaces;
using digioz.Portal.EmailProviders.Models;
using digioz.Portal.EmailProviders.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.EmailProviders.Providers;

/// <summary>
/// Mailgun email provider implementation using REST API
/// </summary>
public class MailgunEmailProvider : IEmailProvider
{
    private readonly MailgunSettings _settings;
    private readonly string _defaultFromEmail;
    private readonly string _defaultFromName;
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;

    public string ProviderName => "Mailgun";

    public MailgunEmailProvider(MailgunSettings settings, string defaultFromEmail, string defaultFromName, ILogger logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _defaultFromEmail = defaultFromEmail;
        _defaultFromName = defaultFromName;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateSettings();

        // Create HttpClient with authentication
        _httpClient = new HttpClient();
        var authBytes = Encoding.ASCII.GetBytes($"api:{_settings.ApiKey}");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
    }

    /// <summary>
    /// Sends an email using Mailgun API
    /// </summary>
    public async Task<EmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var content = CreateMailgunContent(message);
            var url = $"{_settings.ApiBaseUrl}/{_settings.Domain}/messages";

            _logger.LogDebug("Sending email via Mailgun to {Domain}", _settings.Domain);

            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                
                string? messageId = null;
                try
                {
                    using var jsonDoc = JsonDocument.Parse(responseBody);
                    messageId = jsonDoc.RootElement.GetProperty("id").GetString();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not parse message ID from Mailgun response");
                }

                messageId ??= Guid.NewGuid().ToString();

                _logger.LogInformation("Email sent successfully via Mailgun. MessageId: {MessageId}", messageId);
                return EmailResult.Success(messageId, ProviderName);
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var errorMessage = $"Mailgun API error: {response.StatusCode} - {errorBody}";
                _logger.LogError(errorMessage);
                return EmailResult.Failure(errorMessage, ProviderName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via Mailgun: {Message}", ex.Message);
            return EmailResult.Failure($"Mailgun error: {ex.Message}", ProviderName, ex);
        }
    }

    /// <summary>
    /// Validates Mailgun configuration by attempting to access the domain
    /// </summary>
    public async Task<bool> ValidateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey) || string.IsNullOrWhiteSpace(_settings.Domain))
            {
                _logger.LogError("Mailgun configuration invalid: API key or domain is missing");
                return false;
            }

            // Test the API key by making a simple GET request to the domain endpoint
            var url = $"{_settings.ApiBaseUrl}/{_settings.Domain}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Mailgun configuration validated successfully");
                return true;
            }
            else
            {
                _logger.LogWarning("Mailgun validation returned status: {StatusCode}", response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mailgun configuration validation failed: {Message}", ex.Message);
            return false;
        }
    }

    private MultipartFormDataContent CreateMailgunContent(EmailMessage message)
    {
        var content = new MultipartFormDataContent();

        // Set From address
        string fromAddress;
        if (message.From != null && !string.IsNullOrWhiteSpace(message.From.Email))
        {
            fromAddress = !string.IsNullOrWhiteSpace(message.From.Name)
                ? $"{message.From.Name} <{message.From.Email}>"
                : message.From.Email;
        }
        else
        {
            fromAddress = !string.IsNullOrWhiteSpace(_defaultFromName)
                ? $"{_defaultFromName} <{_defaultFromEmail}>"
                : _defaultFromEmail;
        }
        content.Add(new StringContent(fromAddress), "from");

        // Add To recipients
        if (!string.IsNullOrWhiteSpace(message.To.Email))
        {
            var toAddress = !string.IsNullOrWhiteSpace(message.To.Name)
                ? $"{message.To.Name} <{message.To.Email}>"
                : message.To.Email;
            content.Add(new StringContent(toAddress), "to");
        }

        foreach (var toAddress in message.ToAddresses.Where(a => !string.IsNullOrWhiteSpace(a.Email)))
        {
            var address = !string.IsNullOrWhiteSpace(toAddress.Name)
                ? $"{toAddress.Name} <{toAddress.Email}>"
                : toAddress.Email;
            content.Add(new StringContent(address), "to");
        }

        // Add CC recipients
        foreach (var ccAddress in message.CcAddresses.Where(a => !string.IsNullOrWhiteSpace(a.Email)))
        {
            var address = !string.IsNullOrWhiteSpace(ccAddress.Name)
                ? $"{ccAddress.Name} <{ccAddress.Email}>"
                : ccAddress.Email;
            content.Add(new StringContent(address), "cc");
        }

        // Add BCC recipients
        foreach (var bccAddress in message.BccAddresses.Where(a => !string.IsNullOrWhiteSpace(a.Email)))
        {
            var address = !string.IsNullOrWhiteSpace(bccAddress.Name)
                ? $"{bccAddress.Name} <{bccAddress.Email}>"
                : bccAddress.Email;
            content.Add(new StringContent(address), "bcc");
        }

        // Set Subject
        content.Add(new StringContent(message.Subject), "subject");

        // Set ReplyTo
        if (message.ReplyTo != null && !string.IsNullOrWhiteSpace(message.ReplyTo.Email))
        {
            var replyToAddress = !string.IsNullOrWhiteSpace(message.ReplyTo.Name)
                ? $"{message.ReplyTo.Name} <{message.ReplyTo.Email}>"
                : message.ReplyTo.Email;
            content.Add(new StringContent(replyToAddress), "h:Reply-To");
        }

        // Set Body (text and/or HTML)
        if (!string.IsNullOrWhiteSpace(message.TextBody))
        {
            content.Add(new StringContent(message.TextBody), "text");
        }

        if (!string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            content.Add(new StringContent(message.HtmlBody), "html");
        }

        // Add attachments
        foreach (var attachment in message.Attachments)
        {
            var fileContent = new ByteArrayContent(attachment.Content);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(attachment.ContentType);
            content.Add(fileContent, "attachment", attachment.FileName);
        }

        // Add custom headers
        foreach (var header in message.CustomHeaders)
        {
            content.Add(new StringContent(header.Value), $"h:{header.Key}");
        }

        // Add tags
        foreach (var tag in message.Tags.Take(3)) // Mailgun allows max 3 tags
        {
            content.Add(new StringContent(tag.Value), "o:tag");
        }

        // Set tracking options
        if (_settings.EnableTracking)
        {
            content.Add(new StringContent("yes"), "o:tracking");
            content.Add(new StringContent("yes"), "o:tracking-clicks");
            content.Add(new StringContent("yes"), "o:tracking-opens");
        }

        // Set DKIM
        if (_settings.EnableDkim)
        {
            content.Add(new StringContent("yes"), "o:dkim");
        }

        // Set priority based on EmailPriority enum
        switch (message.Priority)
        {
            case EmailPriority.High:
                content.Add(new StringContent("high"), "h:Priority");
                content.Add(new StringContent("1"), "h:X-Priority");
                break;
            case EmailPriority.Low:
                content.Add(new StringContent("low"), "h:Priority");
                content.Add(new StringContent("5"), "h:X-Priority");
                break;
            default: // Normal
                content.Add(new StringContent("normal"), "h:Priority");
                content.Add(new StringContent("3"), "h:X-Priority");
                break;
        }

        return content;
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new EmailConfigurationException("Mailgun API Key is required", ProviderName);
        }

        if (string.IsNullOrWhiteSpace(_settings.Domain))
        {
            throw new EmailConfigurationException("Mailgun Domain is required", ProviderName);
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiBaseUrl))
        {
            _settings.ApiBaseUrl = "https://api.mailgun.net/v3";
        }
    }
}
