using NUnit.Framework;
using FluentAssertions;
using digioz.Portal.Utilities;
using System.Collections.Generic;

namespace digioz.Portal.Tests.Unit.Utilities
{
    /// <summary>
    /// Unit tests for InputSanitizer - Critical security component for input validation
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Utilities")]
    [Category("Security")]
    public class InputSanitizerTests
    {
        #region SanitizeText Tests

        [Test]
        public void SanitizeText_WithNormalText_ReturnsSanitizedText()
        {
            // Arrange
            string input = "Hello World";

            // Act
            var result = InputSanitizer.SanitizeText(input);

            // Assert
            result.Should().Be("Hello World");
        }

        [Test]
        public void SanitizeText_WithHtmlTags_RemovesTags()
        {
            // Arrange
            string input = "<script>alert('XSS')</script>Hello";

            // Act
            var result = InputSanitizer.SanitizeText(input);

            // Assert
            result.Should().NotContain("<script>");
            result.Should().NotContain("XSS");
            result.Should().Be("Hello");
        }

        [Test]
        public void SanitizeText_WithNullInput_ReturnsEmptyString()
        {
            // Arrange
            string? input = null;

            // Act
            var result = InputSanitizer.SanitizeText(input);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void SanitizeText_WithMaxLength_TruncatesToMaxLength()
        {
            // Arrange
            string input = "This is a very long string that should be truncated";

            // Act
            var result = InputSanitizer.SanitizeText(input, maxLength: 20);

            // Assert
            result.Should().HaveLength(20);
        }

        #endregion

        #region SanitizePollQuestion Tests

        [Test]
        public void SanitizePollQuestion_WithValidQuestion_ReturnsQuestion()
        {
            // Arrange
            string input = "What is your favorite color?";

            // Act
            var result = InputSanitizer.SanitizePollQuestion(input);

            // Assert
            result.Should().Be("What is your favorite color?");
        }

        [Test]
        public void SanitizePollQuestion_WithHtmlTags_RemovesTags()
        {
            // Arrange
            string input = "<b>Question</b><script>alert('XSS')</script>";

            // Act
            var result = InputSanitizer.SanitizePollQuestion(input);

            // Assert
            result.Should().NotContain("<b>");
            result.Should().NotContain("<script>");
        }

        [Test]
        public void SanitizePollQuestion_ExceedingMaxLength_TruncatesQuestion()
        {
            // Arrange
            string input = new string('A', 600); // Exceeds 500 char limit

            // Act
            var result = InputSanitizer.SanitizePollQuestion(input);

            // Assert
            result.Should().HaveLength(500);
        }

        #endregion

        #region SanitizePollAnswers Tests

        [Test]
        public void SanitizePollAnswers_WithValidAnswers_ReturnsSanitizedAnswers()
        {
            // Arrange
            var input = new List<string> { "Answer 1", "Answer 2", "Answer 3" };

            // Act
            var result = InputSanitizer.SanitizePollAnswers(input);

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain("Answer 1");
            result.Should().Contain("Answer 2");
            result.Should().Contain("Answer 3");
        }

        [Test]
        public void SanitizePollAnswers_WithEmptyStrings_RemovesEmptyAnswers()
        {
            // Arrange
            var input = new List<string> { "Answer 1", "", "  ", "Answer 2" };

            // Act
            var result = InputSanitizer.SanitizePollAnswers(input);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain("Answer 1");
            result.Should().Contain("Answer 2");
        }

        [Test]
        public void SanitizePollAnswers_WithHtmlTags_RemovesTags()
        {
            // Arrange
            var input = new List<string> { "<b>Answer</b>", "<script>alert('XSS')</script>test" };

            // Act
            var result = InputSanitizer.SanitizePollAnswers(input);

            // Assert
            result.Should().NotContain(s => s.Contains("<b>"));
            result.Should().NotContain(s => s.Contains("<script>"));
        }

        [Test]
        public void SanitizePollAnswers_WithDuplicates_RemovesDuplicates()
        {
            // Arrange
            var input = new List<string> { "Answer 1", "Answer 2", "Answer 1" };

            // Act
            var result = InputSanitizer.SanitizePollAnswers(input);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain("Answer 1");
            result.Should().Contain("Answer 2");
        }

        #endregion

        #region ValidateList Tests

        [Test]
        public void ValidateList_WithValidList_ReturnsNull()
        {
            // Arrange
            var input = new List<string> { "Item 1", "Item 2", "Item 3" };

            // Act
            var result = InputSanitizer.ValidateList(input, "items", minCount: 2, maxCount: 10);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void ValidateList_WithTooFewItems_ReturnsErrorMessage()
        {
            // Arrange
            var input = new List<string> { "Item 1" };

            // Act
            var result = InputSanitizer.ValidateList(input, "items", minCount: 2, maxCount: 10);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("At least 2");
            result.Should().Contain("items are required");
        }

        [Test]
        public void ValidateList_WithTooManyItems_ReturnsErrorMessage()
        {
            // Arrange
            var input = new List<string> { "1", "2", "3", "4", "5", "6" };

            // Act
            var result = InputSanitizer.ValidateList(input, "items", minCount: 1, maxCount: 5);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("No more than 5");
            result.Should().Contain("items are allowed");
        }

        [Test]
        public void ValidateList_WithNullList_ReturnsErrorMessage()
        {
            // Arrange
            List<string>? input = null;

            // Act
            var result = InputSanitizer.ValidateList(input, "items", minCount: 1, maxCount: 10);

            // Assert
            result.Should().NotBeNull();
        }

        #endregion

        #region ValidateString Tests

        [Test]
        public void ValidateString_WithValidString_ReturnsNull()
        {
            // Arrange
            string input = "Valid input";

            // Act
            var result = InputSanitizer.ValidateString(input, "Field", minLength: 1, maxLength: 50);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void ValidateString_WithTooShortString_ReturnsErrorMessage()
        {
            // Arrange
            string input = "Hi";

            // Act
            var result = InputSanitizer.ValidateString(input, "Field", minLength: 5, maxLength: 50);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("at least 5");
        }

        [Test]
        public void ValidateString_WithTooLongString_ReturnsErrorMessage()
        {
            // Arrange
            string input = new string('A', 100);

            // Act
            var result = InputSanitizer.ValidateString(input, "Field", minLength: 1, maxLength: 50);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("not exceed 50");
        }

        #endregion
    }
}
