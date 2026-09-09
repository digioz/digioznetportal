# Forgot Password Email Implementation

This document describes the implementation of the Forgot Password email feature using the digioz.Portal.EmailProviders library.

## Overview

The forgot password feature has been fully integrated with the email providers library, supporting all 4 email providers:
- **SMTP** - Traditional email server
- **SendGrid** - Cloud email service
- **Mailgun** - Cloud email API
- **Azure Communication Services Email** - Microsoft Azure email service

## Implementation Components

### 1. EmailNotificationService (Dal Layer)

**Location**: `digioz.Portal.Dal/Services/EmailNotificationService.cs`

This service provides a clean abstraction for sending emails throughout the portal:

#### Key Features:
- ? Loads email configuration from Config table
- ? **Automatically decrypts encrypted configuration values**
- ? Automatically selects the correct email provider based on `Email:ProviderType`
- ? Backward compatible with legacy SMTP configuration
- ? Professional HTML email templates with responsive design
- ? Plain text alternatives for all emails
- ? Retry logic with exponential backoff (3 attempts)
- ? Comprehensive logging
- ? Security-focused (doesn't reveal user existence)

#### Encryption Support:

The service automatically decrypts sensitive configuration values that are marked as encrypted in the Config table:

**Encrypted Configuration Keys:**
- `Email:Smtp:Password` (or legacy `SMTPPassword`)
- `Email:SendGrid:ApiKey`
- `Email:Mailgun:ApiKey`
- `Email:Azure:ConnectionString`

**How It Works:**
1. Service reads the `SiteEncryptionKey` from `appsettings.json` or `IConfiguration`
2. When loading config values, it checks the `IsEncrypted` flag on each Config record
3. If encrypted, it uses the `EncryptString` utility to decrypt the value
4. Decrypted values are used directly with the email providers
5. If decryption fails, a warning is logged and the default value is used

**Configuration Required:**
```json
{
  "SiteEncryptionKey": "YourSecure32CharacterKeyHere12345"
}
```

**Note**: The encryption key must be 16, 24, or 32 characters (128, 192, or 256 bits for AES encryption).

#### Available Methods:

```csharp
// Send password reset email
Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink);

// Send welcome email to new users
Task<bool> SendWelcomeEmailAsync(string toEmail, string userName, string confirmationLink = null);

// Send email confirmation
Task<bool> SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink);

// Send generic email
Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string textBody = null);

// Validate email configuration
Task<bool> ValidateConfigurationAsync();
```

### 2. Updated ForgotPassword Page

**Location**: `digioz.Portal.Web/Areas/Identity/Pages/Account/ForgotPassword.cshtml.cs`

#### Changes Made:
- Replaced `IEmailSender` with `IEmailNotificationService`
- Added comprehensive error handling
- Added detailed logging for troubleshooting
- Uses professional HTML email template
- Maintains security best practices (doesn't reveal if email exists)

### 3. Dependency Injection Registration

**Location**: `digioz.Portal.Web/Program.cs`

Registered services:
```csharp
// Register Email Provider Services
builder.Services.AddEmailProviders();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
```

### 4. Project References

Updated `digioz.Portal.Dal/digioz.Portal.Dal.csproj` to include:
```xml
<ProjectReference Include="..\digioz.Portal.EmailProviders\digioz.Portal.EmailProviders.csproj" />
```

## Configuration

### Required Config Keys

All email configuration is stored in the `Config` table. The migration `20250723043739_MigrateOtherDataSeed` includes these keys:

#### General Email Settings:
- `Email:ProviderType` - Set to "SMTP", "SendGrid", "Mailgun", or "AzureEmail"
- `Email:FromEmail` - Default sender email address
- `Email:FromName` - Default sender display name
- `Email:IsEnabled` - Enable/disable email globally (true/false)
- `Email:TimeoutSeconds` - Email operation timeout (default: 30)

#### SMTP Provider Settings:
- `Email:Smtp:Host` - SMTP server hostname
- `Email:Smtp:Port` - SMTP port (default: 587)
- `Email:Smtp:Username` - SMTP username
- `Email:Smtp:Password` - SMTP password (**encrypted**, `IsEncrypted = true`)
- `Email:Smtp:EnableSsl` - Enable SSL/TLS (true/false)
- `Email:Smtp:UseDefaultCredentials` - Use Windows auth (true/false)

#### SendGrid Provider Settings:
- `Email:SendGrid:ApiKey` - SendGrid API key (**encrypted**, `IsEncrypted = true`)
- `Email:SendGrid:SandboxMode` - Test mode (true/false)
- `Email:SendGrid:EnableClickTracking` - Track clicks (true/false)
- `Email:SendGrid:EnableOpenTracking` - Track opens (true/false)
- `Email:SendGrid:TemplateId` - Optional template ID

#### Mailgun Provider Settings:
- `Email:Mailgun:ApiKey` - Mailgun API key (**encrypted**, `IsEncrypted = true`)
- `Email:Mailgun:Domain` - Mailgun domain
- `Email:Mailgun:ApiBaseUrl` - API URL (default: https://api.mailgun.net/v3)
- `Email:Mailgun:EnableTracking` - Enable tracking (true/false)
- `Email:Mailgun:EnableDkim` - Enable DKIM (true/false)

#### Azure Email Provider Settings:
- `Email:Azure:ConnectionString` - Azure connection string (**encrypted**, `IsEncrypted = true`)
- `Email:Azure:EnableTracking` - Enable tracking (true/false)

### Encryption Setup

**Step 1: Set Encryption Key in appsettings.json**

```json
{
  "SiteEncryptionKey": "YourSecure32CharacterKeyHere12345",
  "ConnectionStrings": {
    "DefaultConnection": "..."
  }
}
```

**Step 2: Mark Config Values as Encrypted**

When adding/editing sensitive config values in the Admin area, check the "Is Encrypted" checkbox. The system will:
1. Encrypt the value using AES encryption with your `SiteEncryptionKey`
2. Store the encrypted value in the database
3. Set `IsEncrypted = true` on the Config record

**Step 3: EmailNotificationService Automatically Decrypts**

When the service loads configuration:
```csharp
// This automatically decrypts if IsEncrypted = true
var apiKey = GetConfigValueDecrypted("Email:SendGrid:ApiKey", "");
```

### Switching Providers

To switch email providers, simply update the `Email:ProviderType` config value in the database:

```sql
UPDATE Config SET ConfigValue = 'SendGrid' WHERE ConfigKey = 'Email:ProviderType';
-- Options: SMTP, SendGrid, Mailgun, AzureEmail, Azure
```

## Email Templates

### Password Reset Email

The password reset email includes:
- **Professional HTML design** with branded header
- **Clear call-to-action button** for password reset
- **Plain URL fallback** for email clients that block links
- **Expiration notice** (24-hour link expiration)
- **Security notice** if user didn't request reset
- **Responsive design** that works on mobile and desktop
- **Plain text alternative** for compatibility

#### Visual Design:
- Blue color scheme (#007bff)
- Centered, responsive layout
- Professional typography
- Clear spacing and hierarchy

### Future Email Templates

The EmailNotificationService also includes templates for:
- **Welcome Email** - Sent to new users
- **Email Confirmation** - For email verification
- **Generic Email** - For custom notifications

## Security Features

1. **Encrypted Sensitive Data**: 
   - API keys, passwords, and connection strings are encrypted at rest
   - AES encryption with configurable key length (128, 192, or 256 bits)
   - Automatic decryption when loading configuration

2. **No User Enumeration**: Always shows success message, even if email doesn't exist

3. **Secure Token Generation**: Uses ASP.NET Core Identity's built-in token generator

4. **HTTPS Links**: Password reset links use the request scheme (HTTPS in production)

5. **Link Expiration**: Password reset tokens expire after 24 hours

6. **Logging**: All operations logged for audit trail (encrypted values never logged)

## Usage Examples

### Send Password Reset Email:

```csharp
public class AccountService
{
    private readonly IEmailNotificationService _emailService;
    
    public async Task<bool> SendPasswordResetAsync(string email, string resetLink)
    {
        return await _emailService.SendPasswordResetEmailAsync(
            email,
            "John Doe",
            resetLink
        );
    }
}
```

### Send Welcome Email:

```csharp
var success = await _emailNotificationService.SendWelcomeEmailAsync(
    "user@example.com",
    "John Doe",
    confirmationLink: "https://yoursite.com/confirm?token=abc123"
);
```

### Send Custom Email:

```csharp
var success = await _emailNotificationService.SendEmailAsync(
    "user@example.com",
    "Custom Subject",
    "<h1>Hello</h1><p>Custom HTML content</p>",
    "Hello\n\nCustom plain text content"
);
```

### Validate Configuration:

```csharp
var isValid = await _emailNotificationService.ValidateConfigurationAsync();
if (!isValid)
{
    _logger.LogError("Email configuration is invalid!");
}
```

## Error Handling

The implementation includes comprehensive error handling:

```csharp
try
{
    var emailSent = await _emailNotificationService.SendPasswordResetEmailAsync(...);
    
    if (emailSent)
    {
        _logger.LogInformation("Password reset email sent successfully");
    }
    else
    {
        _logger.LogError("Failed to send password reset email");
        // Still redirect to confirmation for security
    }
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error processing password reset");
    // Still redirect to confirmation for security
}
```

### Encryption Error Handling

If encryption/decryption fails:
1. Error is logged with details
2. Default value is used instead
3. Application continues to function (graceful degradation)
4. Admin is notified via logs to fix the encryption key

## Logging

All email operations are logged with appropriate log levels:

- **Information**: Successful email sends, configuration loaded
- **Warning**: Non-existent/unconfirmed emails, config not found, missing encryption key
- **Error**: Email send failures, configuration errors, decryption failures
- **Critical**: N/A (errors don't prevent application from running)

Example log output:
```
[Information] Password reset email sent successfully to user@example.com
[Warning] Password reset requested for non-existent or unconfirmed email: unknown@example.com
[Error] Failed to send password reset email to user@example.com
[Error] Failed to decrypt config key Email:Smtp:Password
[Warning] SiteEncryptionKey not found in configuration. Encrypted values will not be decrypted.
```

## Testing

### Test Password Reset Flow:

1. Navigate to `/Identity/Account/ForgotPassword`
2. Enter a registered email address
3. Click "Submit"
4. Check logs to verify email was sent
5. Check email inbox for password reset message
6. Click the reset link and verify it works

### Test Different Providers:

1. Update `Email:ProviderType` in Config table
2. Ensure provider-specific settings are configured and encrypted (if sensitive)
3. Test forgot password flow
4. Verify email is received from correct provider

### Test Encryption:

1. **Add encrypted config value**:
   - Go to Admin > Config > Add
   - Set ConfigKey: `Test:EncryptedValue`
   - Set ConfigValue: `MySecretValue`
   - Check "Is Encrypted"
   - Save
2. **Verify in database**:
   ```sql
   SELECT ConfigKey, ConfigValue, IsEncrypted FROM Config WHERE ConfigKey = 'Test:EncryptedValue'
   -- ConfigValue should be encrypted (Base64 string)
   ```
3. **Test decryption in code**:
   ```csharp
   var value = GetConfigValueDecrypted("Test:EncryptedValue", "");
   // Should return "MySecretValue"
   ```

### Test Configuration Validation:

```csharp
var emailService = serviceProvider.GetRequiredService<IEmailNotificationService>();
var isValid = await emailService.ValidateConfigurationAsync();
Console.WriteLine($"Email configuration valid: {isValid}");
```

## Troubleshooting

### Email Not Sending

1. **Check `Email:IsEnabled` config** - Must be "true"
2. **Verify provider type** - Check `Email:ProviderType` is correct
3. **Check provider settings** - Ensure all required settings for selected provider are configured
4. **Verify encryption** - Ensure sensitive values are properly encrypted and encryption key is set
5. **Review logs** - Check application logs for error messages
6. **Test provider credentials** - Verify API keys, passwords, etc. are correct

### Decryption Errors

| Issue | Solution |
|-------|----------|
| "Cannot decrypt config key" | Add `SiteEncryptionKey` to appsettings.json |
| "Failed to decrypt config key" | Verify encryption key is correct (same key used to encrypt) |
| "Encryption key has invalid length" | Ensure key is exactly 16, 24, or 32 characters |
| Encrypted value not decrypting | Verify `IsEncrypted = true` in Config table |

### Common Issues:

| Issue | Solution |
|-------|----------|
| Email not received | Check spam folder, verify email address is correct |
| **Gmail rejects with "Messages missing a valid Message-ID header"** | **Fixed in latest version - SMTP provider now generates RFC 5322 compliant Message-ID** |
| SMTP authentication failed | Verify SMTP username/password, check if encrypted correctly |
| SendGrid API error | Verify API key is decrypted correctly, check SendGrid dashboard |
| Mailgun domain error | Verify domain is set up in Mailgun |
| Azure connection failed | Verify connection string decrypts correctly, check Azure resource |

## Best Practices

1. **Always encrypt sensitive configuration** - API keys, passwords, connection strings
2. **Store encryption key securely** - Use Azure Key Vault in production, not source control
3. **Use strong encryption key** - 32 characters for 256-bit AES encryption
4. **Rotate encryption keys** - Periodically update encryption key (requires re-encrypting all values)
5. **Always use retry logic** for production emails
6. **Log all email operations** for audit trail (without exposing encrypted values)
7. **Use HTML + plain text** for best compatibility
8. **Test with multiple email clients** (Gmail, Outlook, etc.)
9. **Monitor email delivery rates** to detect issues early
10. **Use sandbox/test modes** before going to production

## Encryption Key Management

### Development Environment
```json
{
  "SiteEncryptionKey": "Dev32CharacterSecureKeyHere1234"
}
```

### Production Environment (Recommended: Azure Key Vault)
```csharp
// In Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

Then retrieve from Key Vault:
```json
{
  "SiteEncryptionKey": "{{AzureKeyVault:SiteEncryptionKey}}"
}
```

## Related Files

- `digioz.Portal.Dal/Services/EmailNotificationService.cs` - Main service implementation
- `digioz.Portal.Dal/Services/Interfaces/IEmailNotificationService.cs` - Service interface
- `digioz.Portal.Utilities/EncryptString.cs` - Encryption/decryption utility
- `digioz.Portal.Web/Areas/Identity/Pages/Account/ForgotPassword.cshtml.cs` - Forgot password page
- `digioz.Portal.Web/Program.cs` - DI registration
- `digioz.Portal.Web/Data/Migrations/20250723043739_MigrateOtherDataSeed.cs` - Config seed data
- `digioz.Portal.EmailProviders/README.md` - Email providers documentation

## Support

For issues or questions:
1. Check application logs for error messages
2. Review this documentation
3. Consult the Email Providers README
4. Check the GitHub repository issues

---

**Document Version**: 1.1  
**Last Updated**: 2025  
**Author**: DigiOz Development Team  
**Changes**: Added encryption/decryption support for sensitive configuration values
