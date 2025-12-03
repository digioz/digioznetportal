using System;
using System.Threading.Tasks;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.EmailProviders.Interfaces;
using digioz.Portal.EmailProviders.Models;
using digioz.Portal.EmailProviders.Models.Configuration;
using digioz.Portal.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace digioz.Portal.Dal.Services
{
    /// <summary>
    /// Service for sending email notifications through configured email provider
    /// Loads configuration from Config table and uses digioz.Portal.EmailProviders library
    /// </summary>
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly IConfigService _configService;
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly EncryptString _encryptString;
        private string _encryptionKey;

        public EmailNotificationService(
            IEmailService emailService,
            IConfigService configService,
            ILogger<EmailNotificationService> logger,
            IConfiguration configuration)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _encryptString = new EncryptString();
        }

        /// <summary>
        /// Gets the encryption key from configuration (lazy loaded)
        /// </summary>
        private string EncryptionKey
        {
            get
            {
                if (_encryptionKey == null)
                {
                    _encryptionKey = _configuration["SiteEncryptionKey"];
                    if (string.IsNullOrWhiteSpace(_encryptionKey))
                    {
                        _logger.LogWarning("SiteEncryptionKey not found in configuration. Encrypted values will not be decrypted.");
                        _encryptionKey = string.Empty;
                    }
                }
                return _encryptionKey;
            }
        }

        /// <summary>
        /// Sends a password reset email to the specified user
        /// </summary>
        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
        {
            try
            {
                var siteName = GetConfigValue("SiteName", "DigiOz Portal");
                var subject = $"Reset Your Password - {siteName}";

                var htmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                            .content {{ background-color: #f9f9f9; padding: 30px; }}
                            .button {{ display: inline-block; padding: 12px 30px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                            .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <div class=""header"">
                                <h1>Password Reset Request</h1>
                            </div>
                            <div class=""content"">
                                <p>Hello {userName},</p>
                                <p>We received a request to reset your password for your {siteName} account.</p>
                                <p>Click the button below to reset your password:</p>
                                <p style=""text-align: center;"">
                                    <a href=""{resetLink}"" class=""button"">Reset Password</a>
                                </p>
                                <p>Or copy and paste this link into your browser:</p>
                                <p style=""word-break: break-all; color: #007bff;"">{resetLink}</p>
                                <p><strong>This link will expire in 24 hours.</strong></p>
                                <p>If you did not request a password reset, please ignore this email or contact support if you have concerns.</p>
                                <p>Best regards,<br>The {siteName} Team</p>
                            </div>
                            <div class=""footer"">
                                <p>This is an automated email. Please do not reply to this message.</p>
                                <p>&copy; {DateTime.Now.Year} {siteName}. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                var textBody = $@"
Password Reset Request

Hello {userName},

We received a request to reset your password for your {siteName} account.

Click the link below to reset your password:
{resetLink}

This link will expire in 24 hours.

If you did not request a password reset, please ignore this email or contact support if you have concerns.

Best regards,
The {siteName} Team

---
This is an automated email. Please do not reply to this message.
© {DateTime.Now.Year} {siteName}. All rights reserved.
";

                return await SendEmailAsync(toEmail, subject, htmlBody, textBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", toEmail);
                return false;
            }
        }

        /// <summary>
        /// Sends a welcome email to a new user
        /// </summary>
        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName, string confirmationLink = null)
        {
            try
            {
                var siteName = GetConfigValue("SiteName", "DigiOz Portal");
                var subject = $"Welcome to {siteName}!";

                var confirmationSection = !string.IsNullOrEmpty(confirmationLink)
                    ? $@"
                        <p>Before you can fully access your account, please confirm your email address by clicking the button below:</p>
                        <p style=""text-align: center;"">
                            <a href=""{confirmationLink}"" class=""button"">Confirm Email Address</a>
                        </p>"
                    : "";

                var htmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
                            .content {{ background-color: #f9f9f9; padding: 30px; }}
                            .button {{ display: inline-block; padding: 12px 30px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                            .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <div class=""header"">
                                <h1>Welcome to {siteName}!</h1>
                            </div>
                            <div class=""content"">
                                <p>Hello {userName},</p>
                                <p>Thank you for joining {siteName}! We're excited to have you as part of our community.</p>
                                {confirmationSection}
                                <p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>
                                <p>Best regards,<br>The {siteName} Team</p>
                            </div>
                            <div class=""footer"">
                                <p>This is an automated email. Please do not reply to this message.</p>
                                <p>&copy; {DateTime.Now.Year} {siteName}. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                var textBody = $@"
Welcome to {siteName}!

Hello {userName},

Thank you for joining {siteName}! We're excited to have you as part of our community.

{(!string.IsNullOrEmpty(confirmationLink) ? $"Please confirm your email address by clicking this link:\n{confirmationLink}\n\n" : "")}If you have any questions or need assistance, please don't hesitate to contact our support team.

Best regards,
The {siteName} Team

---
This is an automated email. Please do not reply to this message.
© {DateTime.Now.Year} {siteName}. All rights reserved.
";

                return await SendEmailAsync(toEmail, subject, htmlBody, textBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to {Email}", toEmail);
                return false;
            }
        }

        /// <summary>
        /// Sends an email confirmation email
        /// </summary>
        public async Task<bool> SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink)
        {
            try
            {
                var siteName = GetConfigValue("SiteName", "DigiOz Portal");
                var subject = $"Confirm Your Email Address - {siteName}";

                var htmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #17a2b8; color: white; padding: 20px; text-align: center; }}
                            .content {{ background-color: #f9f9f9; padding: 30px; }}
                            .button {{ display: inline-block; padding: 12px 30px; background-color: #17a2b8; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                            .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <div class=""header"">
                                <h1>Confirm Your Email</h1>
                            </div>
                            <div class=""content"">
                                <p>Hello {userName},</p>
                                <p>Please confirm your email address by clicking the button below:</p>
                                <p style=""text-align: center;"">
                                    <a href=""{confirmationLink}"" class=""button"">Confirm Email Address</a>
                                </p>
                                <p>Or copy and paste this link into your browser:</p>
                                <p style=""word-break: break-all; color: #17a2b8;"">{confirmationLink}</p>
                                <p>If you did not create an account with {siteName}, please ignore this email.</p>
                                <p>Best regards,<br>The {siteName} Team</p>
                            </div>
                            <div class=""footer"">
                                <p>This is an automated email. Please do not reply to this message.</p>
                                <p>&copy; {DateTime.Now.Year} {siteName}. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                var textBody = $@"
Confirm Your Email

Hello {userName},

Please confirm your email address by clicking this link:
{confirmationLink}

If you did not create an account with {siteName}, please ignore this email.

Best regards,
The {siteName} Team

---
This is an automated email. Please do not reply to this message.
© {DateTime.Now.Year} {siteName}. All rights reserved.
";

                return await SendEmailAsync(toEmail, subject, htmlBody, textBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email confirmation to {Email}", toEmail);
                return false;
            }
        }

        /// <summary>
        /// Sends a generic email notification
        /// </summary>
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string textBody = null)
        {
            try
            {
                var emailConfig = LoadEmailConfiguration();

                if (!emailConfig.IsEnabled)
                {
                    _logger.LogWarning("Email sending is disabled in configuration");
                    return false;
                }

                var message = new EmailMessage
                {
                    To = new EmailAddress(toEmail),
                    Subject = subject,
                    HtmlBody = htmlBody,
                    TextBody = textBody ?? StripHtml(htmlBody)
                };

                var result = await _emailService.SendEmailWithRetryAsync(message, emailConfig, maxRetries: 3);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Email sent successfully to {Email}. MessageId: {MessageId}", toEmail, result.MessageId);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send email to {Email}: {Error}", toEmail, result.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", toEmail);
                return false;
            }
        }

        /// <summary>
        /// Validates that email configuration is properly set up
        /// </summary>
        public async Task<bool> ValidateConfigurationAsync()
        {
            try
            {
                var emailConfig = LoadEmailConfiguration();
                return await _emailService.ValidateConfigurationAsync(emailConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating email configuration");
                return false;
            }
        }

        /// <summary>
        /// Loads email configuration from Config table
        /// </summary>
        private EmailConfiguration LoadEmailConfiguration()
        {
            var providerType = GetConfigValue("Email:ProviderType", "SMTP");

            var config = new EmailConfiguration
            {
                ProviderType = providerType,
                FromEmail = GetConfigValue("Email:FromEmail", GetConfigValue("WebmasterEmail", "noreply@domain.com")),
                FromName = GetConfigValue("Email:FromName", GetConfigValue("SiteName", "DigiOz Portal")),
                IsEnabled = bool.Parse(GetConfigValue("Email:IsEnabled", "true")),
                TimeoutSeconds = int.Parse(GetConfigValue("Email:TimeoutSeconds", "30"))
            };

            // Load provider-specific settings based on the configured provider
            switch (providerType.ToUpperInvariant())
            {
                case "SENDGRID":
                    config.SendGridSettings = new SendGridSettings
                    {
                        ApiKey = GetConfigValueDecrypted("Email:SendGrid:ApiKey", ""),
                        EnableClickTracking = bool.Parse(GetConfigValue("Email:SendGrid:EnableClickTracking", "true")),
                        EnableOpenTracking = bool.Parse(GetConfigValue("Email:SendGrid:EnableOpenTracking", "true")),
                        SandboxMode = bool.Parse(GetConfigValue("Email:SendGrid:SandboxMode", "false")),
                        TemplateId = GetConfigValue("Email:SendGrid:TemplateId", null)
                    };
                    break;

                case "MAILGUN":
                    config.MailgunSettings = new MailgunSettings
                    {
                        ApiKey = GetConfigValueDecrypted("Email:Mailgun:ApiKey", ""),
                        Domain = GetConfigValue("Email:Mailgun:Domain", ""),
                        ApiBaseUrl = GetConfigValue("Email:Mailgun:ApiBaseUrl", "https://api.mailgun.net/v3"),
                        EnableTracking = bool.Parse(GetConfigValue("Email:Mailgun:EnableTracking", "true")),
                        EnableDkim = bool.Parse(GetConfigValue("Email:Mailgun:EnableDkim", "true"))
                    };
                    break;

                case "AZUREEMAIL":
                case "AZURE":
                    config.AzureEmailSettings = new AzureEmailSettings
                    {
                        ConnectionString = GetConfigValueDecrypted("Email:Azure:ConnectionString", ""),
                        EnableTracking = bool.Parse(GetConfigValue("Email:Azure:EnableTracking", "true"))
                    };
                    break;

                case "SMTP":
                default:
                    // Try new config keys first, fall back to legacy keys for backward compatibility
                    var smtpHost = GetConfigValue("Email:Smtp:Host", GetConfigValue("SMTPServer", "mail.domain.com"));
                    var smtpPort = int.Parse(GetConfigValue("Email:Smtp:Port", GetConfigValue("SMTPPort", "587")));
                    var smtpUsername = GetConfigValue("Email:Smtp:Username", GetConfigValue("SMTPUsername", ""));
                    
                    // Decrypt password - try new key first, then legacy key
                    var smtpPassword = GetConfigValueDecrypted("Email:Smtp:Password", "");
                    if (string.IsNullOrEmpty(smtpPassword))
                    {
                        smtpPassword = GetConfigValueDecrypted("SMTPPassword", "");
                    }

                    config.SmtpSettings = new SmtpSettings
                    {
                        Host = smtpHost,
                        Port = smtpPort,
                        Username = smtpUsername,
                        Password = smtpPassword,
                        EnableSsl = bool.Parse(GetConfigValue("Email:Smtp:EnableSsl", "true")),
                        UseDefaultCredentials = bool.Parse(GetConfigValue("Email:Smtp:UseDefaultCredentials", "false"))
                    };
                    break;
            }

            return config;
        }

        /// <summary>
        /// Gets a configuration value from the Config table
        /// </summary>
        private string GetConfigValue(string key, string defaultValue = "")
        {
            try
            {
                var config = _configService.GetByKey(key);
                return config?.ConfigValue ?? defaultValue;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error retrieving config key {Key}, using default value", key);
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets a configuration value from the Config table and decrypts it if encrypted
        /// </summary>
        private string GetConfigValueDecrypted(string key, string defaultValue = "")
        {
            try
            {
                var config = _configService.GetByKey(key);
                if (config == null)
                {
                    return defaultValue;
                }

                // If the value is encrypted, decrypt it
                if (config.IsEncrypted && !string.IsNullOrEmpty(config.ConfigValue))
                {
                    if (string.IsNullOrEmpty(EncryptionKey))
                    {
                        _logger.LogError("Cannot decrypt config key {Key} - encryption key not configured", key);
                        return defaultValue;
                    }

                    try
                    {
                        var decryptedValue = _encryptString.Decrypt(EncryptionKey, config.ConfigValue);
                        return decryptedValue;
                    }
                    catch (Exception decryptEx)
                    {
                        _logger.LogError(decryptEx, "Failed to decrypt config key {Key}", key);
                        return defaultValue;
                    }
                }

                return config.ConfigValue ?? defaultValue;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error retrieving config key {Key}, using default value", key);
                return defaultValue;
            }
        }

        /// <summary>
        /// Simple HTML stripper for generating plain text from HTML
        /// </summary>
        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // Simple regex to strip HTML tags
            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty)
                .Replace("&nbsp;", " ")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&")
                .Trim();
        }
    }
}
