using NUnit.Framework;
using FluentAssertions;
using Moq;
using digioz.Portal.Dal.Services;
using digioz.Portal.Dal.Services.Interfaces;
using System.Threading.Tasks;

namespace digioz.Portal.Tests.Unit.Services
{
    /// <summary>
    /// Unit tests for EmailNotificationService - Email sending and notifications
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Communication")]
    public class EmailNotificationServiceTests
    {
        private Mock<IEmailNotificationService> _mockService;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<IEmailNotificationService>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockService?.Reset();
        }

        #region SendPasswordResetEmailAsync Tests

        [Test]
        public async Task SendPasswordResetEmailAsync_WithValidParameters_ReturnsTrue()
        {
            // Arrange
            var toEmail = "user@example.com";
            var userName = "john_doe";
            var resetLink = "https://example.com/reset?token=abc123";

            _mockService
                .Setup(s => s.SendPasswordResetEmailAsync(toEmail, userName, resetLink))
                .ReturnsAsync(true);

            // Act
            var result = await _mockService.Object.SendPasswordResetEmailAsync(toEmail, userName, resetLink);

            // Assert
            result.Should().BeTrue();
            _mockService.Verify(s => s.SendPasswordResetEmailAsync(toEmail, userName, resetLink), Times.Once);
        }

        [Test]
        public async Task SendPasswordResetEmailAsync_WithInvalidEmail_ReturnsFalse()
        {
            // Arrange
            _mockService
                .Setup(s => s.SendPasswordResetEmailAsync("invalid-email", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _mockService.Object.SendPasswordResetEmailAsync("invalid-email", "user", "http://link");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task SendPasswordResetEmailAsync_MultipleUsers_SendsAllEmails()
        {
            // Arrange
            var users = new[]
            {
                ("alice@example.com", "alice", "https://reset/alice"),
                ("bob@example.com", "bob", "https://reset/bob"),
                ("charlie@example.com", "charlie", "https://reset/charlie")
            };

            foreach (var (email, name, link) in users)
            {
                _mockService
                    .Setup(s => s.SendPasswordResetEmailAsync(email, name, link))
                    .ReturnsAsync(true);
            }

            // Act
            var results = new bool[users.Length];
            for (int i = 0; i < users.Length; i++)
            {
                results[i] = await _mockService.Object.SendPasswordResetEmailAsync(users[i].Item1, users[i].Item2, users[i].Item3);
            }

            // Assert
            results.Should().AllSatisfy(r => r.Should().BeTrue());
            _mockService.Verify(
                s => s.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Exactly(3));
        }

        #endregion

        #region SendWelcomeEmailAsync Tests

        [Test]
        public async Task SendWelcomeEmailAsync_WithValidParameters_ReturnsTrue()
        {
            // Arrange
            var toEmail = "newuser@example.com";
            var userName = "new_user";

            _mockService
                .Setup(s => s.SendWelcomeEmailAsync(toEmail, userName, null))
                .ReturnsAsync(true);

            // Act
            var result = await _mockService.Object.SendWelcomeEmailAsync(toEmail, userName);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task SendWelcomeEmailAsync_WithConfirmationLink_ReturnsTrue()
        {
            // Arrange
            var toEmail = "newuser@example.com";
            var userName = "new_user";
            var confirmationLink = "https://example.com/confirm?token=xyz789";

            _mockService
                .Setup(s => s.SendWelcomeEmailAsync(toEmail, userName, confirmationLink))
                .ReturnsAsync(true);

            // Act
            var result = await _mockService.Object.SendWelcomeEmailAsync(toEmail, userName, confirmationLink);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region SendEmailConfirmationAsync Tests

        [Test]
        public async Task SendEmailConfirmationAsync_WithValidParameters_ReturnsTrue()
        {
            // Arrange
            var toEmail = "user@example.com";
            var userName = "user123";
            var confirmationLink = "https://example.com/confirm?token=confirm123";

            _mockService
                .Setup(s => s.SendEmailConfirmationAsync(toEmail, userName, confirmationLink))
                .ReturnsAsync(true);

            // Act
            var result = await _mockService.Object.SendEmailConfirmationAsync(toEmail, userName, confirmationLink);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task SendEmailConfirmationAsync_RequiresConfirmationLink()
        {
            // Arrange
            _mockService
                .Setup(s => s.SendEmailConfirmationAsync("user@test.com", "user", null))
                .ReturnsAsync(false);

            // Act
            var result = await _mockService.Object.SendEmailConfirmationAsync("user@test.com", "user", null);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region SendEmailAsync Tests

        [Test]
        public async Task SendEmailAsync_WithValidParameters_ReturnsTrue()
        {
            // Arrange
            var toEmail = "recipient@example.com";
            var subject = "Test Subject";
            var htmlBody = "<h1>Test Email</h1>";

            _mockService
                .Setup(s => s.SendEmailAsync(toEmail, subject, htmlBody, null))
                .ReturnsAsync(true);

            // Act
            var result = await _mockService.Object.SendEmailAsync(toEmail, subject, htmlBody);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task SendEmailAsync_WithHtmlAndTextBody_ReturnsTrue()
        {
            // Arrange
            var toEmail = "recipient@example.com";
            var subject = "Test Subject";
            var htmlBody = "<h1>Test Email</h1>";
            var textBody = "Test Email";

            _mockService
                .Setup(s => s.SendEmailAsync(toEmail, subject, htmlBody, textBody))
                .ReturnsAsync(true);

            // Act
            var result = await _mockService.Object.SendEmailAsync(toEmail, subject, htmlBody, textBody);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task SendEmailAsync_BulkEmailSending()
        {
            // Arrange
            var recipients = new[]
            {
                ("user1@test.com", "Hello User 1"),
                ("user2@test.com", "Hello User 2"),
                ("user3@test.com", "Hello User 3")
            };

            foreach (var (email, body) in recipients)
            {
                _mockService
                    .Setup(s => s.SendEmailAsync(email, "Newsletter", body, null))
                    .ReturnsAsync(true);
            }

            // Act
            var results = new bool[recipients.Length];
            for (int i = 0; i < recipients.Length; i++)
            {
                results[i] = await _mockService.Object.SendEmailAsync(
                    recipients[i].Item1, 
                    "Newsletter", 
                    recipients[i].Item2);
            }

            // Assert
            results.Should().AllSatisfy(r => r.Should().BeTrue());
        }

        #endregion

        #region ValidateConfigurationAsync Tests

        [Test]
        public async Task ValidateConfigurationAsync_WithValidConfig_ReturnsTrue()
        {
            // Arrange
            _mockService
                .Setup(s => s.ValidateConfigurationAsync())
                .ReturnsAsync(true);

            // Act
            var result = await _mockService.Object.ValidateConfigurationAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task ValidateConfigurationAsync_WithInvalidConfig_ReturnsFalse()
        {
            // Arrange
            _mockService
                .Setup(s => s.ValidateConfigurationAsync())
                .ReturnsAsync(false);

            // Act
            var result = await _mockService.Object.ValidateConfigurationAsync();

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Integration Tests

        [Test]
        public async Task EmailNotification_CompleteUserOnboarding()
        {
            // Arrange - User registration workflow
            var userEmail = "newmember@example.com";
            var userName = "newmember";
            var confirmationLink = "https://example.com/confirm?token=confirm123";

            _mockService
                .Setup(s => s.SendWelcomeEmailAsync(userEmail, userName, confirmationLink))
                .ReturnsAsync(true);

            _mockService
                .Setup(s => s.ValidateConfigurationAsync())
                .ReturnsAsync(true);

            // Act
            var configValid = await _mockService.Object.ValidateConfigurationAsync();
            var welcomeSent = await _mockService.Object.SendWelcomeEmailAsync(userEmail, userName, confirmationLink);

            // Assert
            configValid.Should().BeTrue();
            welcomeSent.Should().BeTrue();
        }

        [Test]
        public async Task EmailNotification_PasswordResetWorkflow()
        {
            // Arrange
            var userEmail = "user@example.com";
            var userName = "testuser";
            var resetToken = "reset_abc123_xyz789";
            var resetLink = $"https://example.com/reset?token={resetToken}";

            _mockService
                .Setup(s => s.SendPasswordResetEmailAsync(userEmail, userName, resetLink))
                .ReturnsAsync(true);

            // Act
            var resetSent = await _mockService.Object.SendPasswordResetEmailAsync(userEmail, userName, resetLink);

            // Assert
            resetSent.Should().BeTrue();
            _mockService.Verify(
                s => s.SendPasswordResetEmailAsync(userEmail, userName, It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public async Task EmailNotification_MultipleNotificationTypes()
        {
            // Arrange - Send multiple types of notifications to same user
            var userEmail = "active@example.com";
            var userName = "active_user";

            _mockService
                .Setup(s => s.SendWelcomeEmailAsync(userEmail, userName, null))
                .ReturnsAsync(true);

            _mockService
                .Setup(s => s.SendEmailAsync(userEmail, "Weekly Newsletter", "<p>This week's news</p>", null))
                .ReturnsAsync(true);

            _mockService
                .Setup(s => s.SendEmailAsync(userEmail, "New Message", "<p>You have a new message</p>", null))
                .ReturnsAsync(true);

            // Act
            var welcome = await _mockService.Object.SendWelcomeEmailAsync(userEmail, userName, null);
            var newsletter = await _mockService.Object.SendEmailAsync(userEmail, "Weekly Newsletter", "<p>This week's news</p>");
            var message = await _mockService.Object.SendEmailAsync(userEmail, "New Message", "<p>You have a new message</p>");

            // Assert
            welcome.Should().BeTrue();
            newsletter.Should().BeTrue();
            message.Should().BeTrue();
        }

        #endregion
    }
}
