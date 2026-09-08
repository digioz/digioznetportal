using System.Threading.Tasks;

namespace digioz.Portal.Dal.Services.Interfaces
{
    /// <summary>
    /// Service for sending email notifications through configured email provider
    /// </summary>
    public interface IEmailNotificationService
    {
        /// <summary>
        /// Sends a password reset email to the specified user
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="userName">User's name or username</param>
        /// <param name="resetLink">Password reset link URL</param>
        /// <returns>True if email was sent successfully, false otherwise</returns>
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink);

        /// <summary>
        /// Sends a welcome email to a new user
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="userName">User's name or username</param>
        /// <param name="confirmationLink">Optional email confirmation link</param>
        /// <returns>True if email was sent successfully, false otherwise</returns>
        Task<bool> SendWelcomeEmailAsync(string toEmail, string userName, string confirmationLink = null);

        /// <summary>
        /// Sends an email confirmation email
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="userName">User's name or username</param>
        /// <param name="confirmationLink">Email confirmation link URL</param>
        /// <returns>True if email was sent successfully, false otherwise</returns>
        Task<bool> SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink);

        /// <summary>
        /// Sends a generic email notification
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlBody">Email HTML body</param>
        /// <param name="textBody">Optional plain text body</param>
        /// <returns>True if email was sent successfully, false otherwise</returns>
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string textBody = null);

        /// <summary>
        /// Validates that email configuration is properly set up
        /// </summary>
        /// <returns>True if email configuration is valid, false otherwise</returns>
        Task<bool> ValidateConfigurationAsync();
    }
}
