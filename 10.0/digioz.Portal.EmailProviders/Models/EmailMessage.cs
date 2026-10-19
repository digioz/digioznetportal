namespace digioz.Portal.EmailProviders.Models;

/// <summary>
/// Represents an email message to be sent
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// Primary recipient (convenience property, also added to ToAddresses)
    /// </summary>
    public EmailAddress To { get; set; } = new();

    /// <summary>
    /// List of primary recipients
    /// </summary>
    public List<EmailAddress> ToAddresses { get; set; } = [];

    /// <summary>
    /// List of CC recipients
    /// </summary>
    public List<EmailAddress> CcAddresses { get; set; } = [];

    /// <summary>
    /// List of BCC recipients
    /// </summary>
    public List<EmailAddress> BccAddresses { get; set; } = [];

    /// <summary>
    /// From address (overrides default from configuration if set)
    /// </summary>
    public EmailAddress? From { get; set; }

    /// <summary>
    /// Reply-to address
    /// </summary>
    public EmailAddress? ReplyTo { get; set; }

    /// <summary>
    /// Email subject
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Plain text body
    /// </summary>
    public string? TextBody { get; set; }

    /// <summary>
    /// HTML body
    /// </summary>
    public string? HtmlBody { get; set; }

    /// <summary>
    /// Email attachments
    /// </summary>
    public List<EmailAttachment> Attachments { get; set; } = [];

    /// <summary>
    /// Email priority
    /// </summary>
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;

    /// <summary>
    /// Custom headers to include in the email
    /// </summary>
    public Dictionary<string, string> CustomHeaders { get; set; } = [];

    /// <summary>
    /// Tags for categorization and tracking (provider-dependent)
    /// </summary>
    public Dictionary<string, string> Tags { get; set; } = [];

    /// <summary>
    /// Validates the email message
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = [];

        if (string.IsNullOrWhiteSpace(Subject))
        {
            errors.Add("Subject is required");
        }

        if (string.IsNullOrWhiteSpace(TextBody) && string.IsNullOrWhiteSpace(HtmlBody))
        {
            errors.Add("Either TextBody or HtmlBody is required");
        }

        var hasRecipients = !string.IsNullOrWhiteSpace(To.Email) || 
                           ToAddresses.Any() || 
                           CcAddresses.Any() || 
                           BccAddresses.Any();

        if (!hasRecipients)
        {
            errors.Add("At least one recipient is required");
        }

        return errors.Count == 0;
    }
}
