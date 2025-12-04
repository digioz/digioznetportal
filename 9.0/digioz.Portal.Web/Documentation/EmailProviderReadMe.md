# DigiOz Portal Email Providers

A reusable email provider library supporting multiple email service providers.

## Supported Providers

- ? **SMTP** - Fully functional
- ? **SendGrid** - Fully functional
- ? **Mailgun** - Fully functional
- ? **Azure Communication Services Email** - Fully functional

## Installation

### Register Services in DI Container

```csharp
// In your Program.cs or Startup.cs
services.AddEmailProviders();
```

## Usage Examples

### SMTP Configuration

Store these values in your Config table:

| ConfigKey | ConfigValue | Description |
|-----------|-------------|-------------|
| `Email:ProviderType` | `SMTP` | Provider to use |
| `Email:FromEmail` | `noreply@yoursite.com` | Default from email |
| `Email:FromName` | `Your Site Name` | Default from name |
| `Email:IsEnabled` | `true` | Enable/disable emails |
| `Email:Smtp:Host` | `smtp.gmail.com` | SMTP server host |
| `Email:Smtp:Port` | `587` | SMTP server port |
| `Email:Smtp:Username` | `your-email@gmail.com` | SMTP username |
| `Email:Smtp:Password` | `your-password` | SMTP password |
| `Email:Smtp:EnableSsl` | `true` | Enable SSL/TLS |
| `Email:Smtp:UseDefaultCredentials` | `false` | Use default credentials |

### SendGrid Configuration

Store these values in your Config table:

| ConfigKey | ConfigValue | Description |
|-----------|-------------|-------------|
| `Email:ProviderType` | `SendGrid` | Provider to use |
| `Email:FromEmail` | `noreply@yoursite.com` | Default from email |
| `Email:FromName` | `Your Site Name` | Default from name |
| `Email:IsEnabled` | `true` | Enable/disable emails |
| `Email:SendGrid:ApiKey` | `SG.xxxxx` | SendGrid API key |
| `Email:SendGrid:SandboxMode` | `false` | Sandbox mode for testing |
| `Email:SendGrid:EnableClickTracking` | `true` | Track email clicks |
| `Email:SendGrid:EnableOpenTracking` | `true` | Track email opens |
| `Email:SendGrid:TemplateId` | `d-xxxxx` | Optional: Template ID |

### Mailgun Configuration

Store these values in your Config table:

| ConfigKey | ConfigValue | Description |
|-----------|-------------|-------------|
| `Email:ProviderType` | `Mailgun` | Provider to use |
| `Email:FromEmail` | `noreply@yoursite.com` | Default from email |
| `Email:FromName` | `Your Site Name` | Default from name |
| `Email:IsEnabled` | `true` | Enable/disable emails |
| `Email:Mailgun:ApiKey` | `key-xxxxx` | Mailgun API key |
| `Email:Mailgun:Domain` | `mg.yoursite.com` | Mailgun domain |
| `Email:Mailgun:ApiBaseUrl` | `https://api.mailgun.net/v3` | API base URL (US) |
| `Email:Mailgun:EnableTracking` | `true` | Enable tracking |
| `Email:Mailgun:EnableDkim` | `true` | Enable DKIM signatures |

**Note**: For EU region, use `https://api.eu.mailgun.net/v3` as the ApiBaseUrl.

### Azure Communication Services Email Configuration

Store these values in your Config table:

| ConfigKey | ConfigValue | Description |
|-----------|-------------|-------------|
| `Email:ProviderType` | `AzureEmail` or `Azure` | Provider to use |
| `Email:FromEmail` | `noreply@yoursite.com` | Default from email (must be verified in Azure) |
| `Email:FromName` | `Your Site Name` | Default from name |
| `Email:IsEnabled` | `true` | Enable/disable emails |
| `Email:Azure:ConnectionString` | `endpoint=https://...;accesskey=...` | Azure Communication Services connection string |
| `Email:Azure:EnableTracking` | `true` | Enable user engagement tracking |

**Note**: The from email address must be a verified domain in your Azure Communication Services Email resource.

### Send Email Example

```csharp
public class EmailNotificationService
{
    private readonly IEmailService _emailService;
    private readonly IConfigService _configService;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IEmailService emailService, 
        IConfigService configService,
        ILogger<EmailNotificationService> logger)
    {
        _emailService = emailService;
        _configService = configService;
        _logger = logger;
    }

    public async Task<bool> SendWelcomeEmail(string toEmail, string userName)
    {
        try
        {
            // Load configuration from Config table
            var emailConfig = LoadEmailConfiguration();

            // Create email message
            var message = new EmailMessage
            {
                To = new EmailAddress(toEmail, userName),
                Subject = "Welcome to DigiOz Portal!",
                HtmlBody = $@"
                    <html>
                    <body>
                        <h1>Welcome {userName}!</h1>
                        <p>Thank you for joining our portal.</p>
                        <p>Get started by visiting your dashboard.</p>
                    </body>
                    </html>",
                TextBody = $"Welcome {userName}! Thank you for joining our portal."
            };

            // Send with retry logic (3 attempts with exponential backoff)
            var result = await _emailService.SendEmailWithRetryAsync(message, emailConfig);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Welcome email sent to {Email}. MessageId: {MessageId}", 
                    toEmail, 
                    result.MessageId);
                return true;
            }
            else
            {
                _logger.LogError(
                    "Failed to send welcome email to {Email}: {Error}", 
                    toEmail, 
                    result.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to {Email}", toEmail);
            return false;
        }
    }

    private EmailConfiguration LoadEmailConfiguration()
    {
        var providerType = _configService.GetValue("Email:ProviderType", "SMTP");

        var config = new EmailConfiguration
        {
            ProviderType = providerType,
            FromEmail = _configService.GetValue("Email:FromEmail", "noreply@digioz.net"),
            FromName = _configService.GetValue("Email:FromName", "DigiOz Portal"),
            IsEnabled = bool.Parse(_configService.GetValue("Email:IsEnabled", "true")),
            TimeoutSeconds = int.Parse(_configService.GetValue("Email:TimeoutSeconds", "30"))
        };

        // Load provider-specific settings
        if (providerType.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
        {
            config.SendGridSettings = new SendGridSettings
            {
                ApiKey = _configService.GetValue("Email:SendGrid:ApiKey", ""),
                EnableClickTracking = bool.Parse(_configService.GetValue("Email:SendGrid:EnableClickTracking", "true")),
                EnableOpenTracking = bool.Parse(_configService.GetValue("Email:SendGrid:EnableOpenTracking", "true")),
                SandboxMode = bool.Parse(_configService.GetValue("Email:SendGrid:SandboxMode", "false")),
                TemplateId = _configService.GetValue("Email:SendGrid:TemplateId", null)
            };
        }
        else if (providerType.Equals("Mailgun", StringComparison.OrdinalIgnoreCase))
        {
            config.MailgunSettings = new MailgunSettings
            {
                ApiKey = _configService.GetValue("Email:Mailgun:ApiKey", ""),
                Domain = _configService.GetValue("Email:Mailgun:Domain", ""),
                ApiBaseUrl = _configService.GetValue("Email:Mailgun:ApiBaseUrl", "https://api.mailgun.net/v3"),
                EnableTracking = bool.Parse(_configService.GetValue("Email:Mailgun:EnableTracking", "true")),
                EnableDkim = bool.Parse(_configService.GetValue("Email:Mailgun:EnableDkim", "true"))
            };
        }
        else if (providerType.Equals("AzureEmail", StringComparison.OrdinalIgnoreCase) || 
                 providerType.Equals("Azure", StringComparison.OrdinalIgnoreCase))
        {
            config.AzureEmailSettings = new AzureEmailSettings
            {
                ConnectionString = _configService.GetValue("Email:Azure:ConnectionString", ""),
                EnableTracking = bool.Parse(_configService.GetValue("Email:Azure:EnableTracking", "true"))
            };
        }
        else if (providerType.Equals("SMTP", StringComparison.OrdinalIgnoreCase))
        {
            config.SmtpSettings = new SmtpSettings
            {
                Host = _configService.GetValue("Email:Smtp:Host", "smtp.gmail.com"),
                Port = int.Parse(_configService.GetValue("Email:Smtp:Port", "587")),
                Username = _configService.GetValue("Email:Smtp:Username", ""),
                Password = _configService.GetValue("Email:Smtp:Password", ""),
                EnableSsl = bool.Parse(_configService.GetValue("Email:Smtp:EnableSsl", "true")),
                UseDefaultCredentials = bool.Parse(_configService.GetValue("Email:Smtp:UseDefaultCredentials", "false"))
            };
        }

        return config;
    }
}
```

### Advanced Features

#### Multiple Recipients

```csharp
var message = new EmailMessage
{
    ToAddresses = new List<EmailAddress>
    {
        new EmailAddress("user1@example.com", "User One"),
        new EmailAddress("user2@example.com", "User Two")
    },
    CcAddresses = new List<EmailAddress>
    {
        new EmailAddress("manager@example.com", "Manager")
    },
    BccAddresses = new List<EmailAddress>
    {
        new EmailAddress("admin@example.com", "Admin")
    },
    Subject = "Team Update",
    HtmlBody = "<p>Hello team!</p>"
};
```

#### Email with Attachments

```csharp
var pdfBytes = await File.ReadAllBytesAsync("document.pdf");

var message = new EmailMessage
{
    To = new EmailAddress("user@example.com"),
    Subject = "Document Attached",
    HtmlBody = "<p>Please see the attached document.</p>",
    Attachments = new List<EmailAttachment>
    {
        new EmailAttachment("document.pdf", pdfBytes, "application/pdf")
    }
};
```

#### Custom Headers and Tags

```csharp
var message = new EmailMessage
{
    To = new EmailAddress("user@example.com"),
    Subject = "Tagged Email",
    HtmlBody = "<p>Email with tags</p>",
    CustomHeaders = new Dictionary<string, string>
    {
        { "X-Campaign-Id", "welcome-2024" }
    },
    Tags = new Dictionary<string, string>
    {
        { "campaign", "welcome" },
        { "type", "transactional" }
    }
};
```

#### Using SendGrid Templates

```csharp
// Configure template ID in settings
var config = new EmailConfiguration
{
    ProviderType = "SendGrid",
    SendGridSettings = new SendGridSettings
    {
        ApiKey = "SG.xxxxx",
        TemplateId = "d-12345abcde"  // Your SendGrid template ID
    }
};

// Pass template variables via Tags
var message = new EmailMessage
{
    To = new EmailAddress("user@example.com", "John Doe"),
    Subject = "Welcome Email",  // May be overridden by template
    Tags = new Dictionary<string, string>
    {
        { "first_name", "John" },
        { "activation_link", "https://yoursite.com/activate/abc123" }
    }
};
```

## Provider-Specific Features

### SMTP Features

- ? Multiple recipients (To, CC, BCC)
- ? HTML and plain text content
- ? File attachments
- ? Custom headers
- ? Reply-To address
- ? Priority levels
- ? SSL/TLS encryption
- ? Alternative views for multipart emails
- ? Inline images (via ContentId)
- ? **RFC 5322 compliant Message-ID header** (required by Gmail and other providers)

### SendGrid Features

- ? Multiple recipients (To, CC, BCC)
- ? HTML and plain text content
- ? File attachments (Base64 encoded)
- ? Custom headers
- ? Categories/Tags for tracking
- ? Click tracking
- ? Open tracking
- ? Sandbox mode (for testing without sending)
- ? Template support
- ? Reply-To address
- ? Priority levels
- ? Inline images (via ContentId)

### Mailgun Features

- ? Multiple recipients (To, CC, BCC)
- ? HTML and plain text content
- ? File attachments
- ? Custom headers (via h: prefix)
- ? Tags for tracking (max 3 tags)
- ? Click tracking
- ? Open tracking
- ? DKIM signatures
- ? Reply-To address
- ? Priority levels
- ? US and EU region support

### Azure Communication Services Email Features

- ? Multiple recipients (To, CC, BCC)
- ? HTML and plain text content
- ? File attachments
- ? Custom headers
- ? User engagement tracking
- ? Reply-To address
- ? Priority levels (via custom headers)
- ? Inline images (via ContentId)
- ? Long-running operations with status tracking
- ? Enterprise-grade reliability and scale

## Testing

### SendGrid Sandbox Mode

Enable sandbox mode to test without actually sending emails:

```csharp
config.SendGridSettings.SandboxMode = true;
```

This will validate the email format and API call but won't deliver the email.

### Mailgun Testing

Mailgun provides a sandbox domain for testing. Use your sandbox domain in the configuration:

```csharp
config.MailgunSettings.Domain = "sandbox1234567890abcdef.mailgun.org";
```

### Azure Communication Services Testing

Azure provides email sending from verified domains. For testing:

1. Set up an Azure Communication Services Email resource
2. Verify your domain or use a free Azure subdomain
3. Use the connection string from your resource

## Error Handling

The library provides comprehensive error handling:

```csharp
var result = await _emailService.SendEmailAsync(message, config);

if (result.IsSuccess)
{
    Console.WriteLine($"Email sent! Message ID: {result.MessageId}");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
    if (result.Exception != null)
    {
        Console.WriteLine($"Exception: {result.Exception.Message}");
    }
}
```

## Configuration Validation

Validate configuration before sending:

```csharp
var isValid = await _emailService.ValidateConfigurationAsync(config);
if (!isValid)
{
    Console.WriteLine("Email configuration is invalid!");
}
```

## Architecture

```
???????????????????????
?  Your Application   ?
???????????????????????
           ?
           ? Uses
           ?
    ????????????????
    ? IEmailService?
    ????????????????
           ?
           ? Creates Provider
           ?
????????????????????????
?IEmailProviderFactory ?
????????????????????????
           ?
           ? Returns
           ?
    ????????????????
    ? IEmailProvider?
    ????????????????
           ?
    ??????????????????????????????????????????????
    ?                      ?          ?          ?
??????????          ????????????  ???????????  ????????
? SMTP   ?          ?SendGrid  ?  ?Mailgun  ?  ?Azure ?
??????????          ????????????  ???????????  ????????
```

## NuGet Dependencies

- `Microsoft.Extensions.Logging.Abstractions` (9.0.0)
- `Microsoft.Extensions.DependencyInjection.Abstractions` (9.0.0)
- `SendGrid` (9.29.3)
- `Azure.Communication.Email` (1.1.0)

**Note**: SMTP and Mailgun use built-in .NET libraries (System.Net.Mail and HttpClient), so no additional packages are required.

## Provider Comparison

| Feature | SMTP | SendGrid | Mailgun | Azure Email |
|---------|------|----------|---------|-------------|
| **Setup Complexity** | Easy | Medium | Medium | Medium |
| **Cost** | Free (self-hosted) | Paid | Paid | Paid |
| **Tracking** | No | Yes | Yes | Yes |
| **Templates** | No | Yes | No | No |
| **Max Tags** | N/A | Unlimited | 3 | N/A |
| **Attachments** | ? | ? | ? | ? |
| **Priority** | ? | ? | ? | ? (via headers) |
| **Sandbox Mode** | N/A | ? | ? (via sandbox domain) | N/A |
| **Best For** | Self-hosted, simple | Marketing emails | Transactional emails | Enterprise Azure users |

## Switching Between Providers

Simply change the `Email:ProviderType` configuration value to switch providers:

```csharp
// Switch to SMTP
_configService.SetValue("Email:ProviderType", "SMTP");

// Switch to SendGrid
_configService.SetValue("Email:ProviderType", "SendGrid");

// Switch to Mailgun
_configService.SetValue("Email:ProviderType", "Mailgun");

// Switch to Azure Email
_configService.SetValue("Email:ProviderType", "AzureEmail");
// or
_configService.SetValue("Email:ProviderType", "Azure");
```

All provider-specific settings are loaded dynamically based on the selected provider.

## Azure Communication Services Setup

To use Azure Communication Services Email:

1. **Create an Azure Communication Services resource** in the Azure Portal
2. **Set up Email Communication Service** and link it to your Communication Services resource
3. **Verify a custom domain** or use the free Azure subdomain
4. **Get the connection string** from your resource's Keys section
5. **Configure the from address** to match your verified domain

Example connection string format:
```
endpoint=https://your-resource.communication.azure.com/;accesskey=your-access-key
```

## Best Practices

1. **Use Retry Logic**: Always use `SendEmailWithRetryAsync` for production code
2. **Validate Configuration**: Call `ValidateConfigurationAsync` during application startup
3. **Log Results**: Always log email send results for troubleshooting
4. **Store Sensitive Data Securely**: Keep API keys and passwords in secure configuration (Azure Key Vault, etc.)
5. **Test Thoroughly**: Use sandbox modes when available before production deployment
6. **Monitor Limits**: Be aware of rate limits and quotas for each provider
7. **Handle Failures Gracefully**: Always check `EmailResult.IsSuccess` and handle failures

## License

This library is part of the DigiOz Portal project.
