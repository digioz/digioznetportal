using NUnit.Framework;
using FluentAssertions;
using digioz.Portal.Utilities;
using System;

namespace digioz.Portal.Tests.Unit.Utilities
{
    /// <summary>
    /// Unit tests for StringUtils - Critical string manipulation and sanitization utilities
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Utilities")]
    [Category("Security")]
    public class StringUtilsTests
    {
        #region IsNullEmpty Tests

        [Test]
        public void IsNullEmpty_WithNullString_ReturnsTrue()
        {
            // Arrange
            string? input = null;

            // Act
            var result = input.IsNullEmpty();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsNullEmpty_WithEmptyString_ReturnsTrue()
        {
            // Arrange
            string input = string.Empty;

            // Act
            var result = input.IsNullEmpty();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsNullEmpty_WithNonEmptyString_ReturnsFalse()
        {
            // Arrange
            string input = "test";

            // Act
            var result = input.IsNullEmpty();

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region RemoveLineBreaks Tests

        [Test]
        public void RemoveLineBreaks_WithCRLF_RemovesLineBreaks()
        {
            // Arrange
            string input = "Line1\r\nLine2\r\nLine3";

            // Act
            var result = input.RemoveLineBreaks();

            // Assert
            result.Should().Be("Line1Line2Line3");
        }

        [Test]
        public void RemoveLineBreaks_WithMixedLineBreaks_RemovesAllLineBreaks()
        {
            // Arrange
            string input = "Line1\rLine2\nLine3\r\nLine4";

            // Act
            var result = input.RemoveLineBreaks();

            // Assert
            result.Should().Be("Line1Line2Line3Line4");
        }

        #endregion

        #region ConvertLineBreaksToHtml Tests

        [Test]
        public void ConvertLineBreaksToHtml_WithCRLF_ConvertsToBrTags()
        {
            // Arrange
            string input = "Line1\r\nLine2\r\nLine3";

            // Act
            var result = input.ConvertLineBreaksToHtml();

            // Assert
            result.Should().Be("Line1<br />Line2<br />Line3");
        }

        [Test]
        public void ConvertLineBreaksToHtml_WithMixedLineBreaks_ConvertsToBrTags()
        {
            // Arrange
            string input = "Line1\rLine2\nLine3";

            // Act
            var result = input.ConvertLineBreaksToHtml();

            // Assert
            result.Should().Be("Line1<br />Line2<br />Line3");
        }

        [Test]
        public void ConvertLineBreaksToHtml_WithEmptyString_ReturnsEmptyString()
        {
            // Arrange
            string input = string.Empty;

            // Act
            var result = input.ConvertLineBreaksToHtml();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region IsValidEmail Tests

        [Test]
        [TestCase("user@example.com", true)]
        [TestCase("user.name@example.com", true)]
        [TestCase("user+tag@example.co.uk", true)]
        [TestCase("invalid.email", false)]
        [TestCase("@example.com", false)]
        [TestCase("user@", false)]
        [TestCase("", false)]
        public void IsValidEmail_WithVariousInputs_ReturnsExpectedResult(string email, bool expected)
        {
            // Act
            var result = StringUtils.IsValidEmail(email);

            // Assert
            result.Should().Be(expected);
        }

        #endregion

        #region ScrubHtml Tests

        [Test]
        public void ScrubHtml_WithScriptTag_RemovesScript()
        {
            // Arrange
            string input = "<p>Hello</p><script>alert('XSS');</script>";

            // Act
            var result = StringUtils.ScrubHtml(input);

            // Assert
            result.Should().NotContain("<script>");
            result.Should().NotContain("alert");
            result.Should().Contain("<p>Hello</p>");
        }

        [Test]
        public void ScrubHtml_WithOnClickHandler_RemovesHandler()
        {
            // Arrange
            string input = "<button onclick=\"alert('XSS')\">Click</button>";

            // Act
            var result = StringUtils.ScrubHtml(input);

            // Assert
            result.Should().NotContain("onclick");
            result.Should().Contain("<button");
            result.Should().Contain("Click");
        }

        [Test]
        public void ScrubHtml_WithJavascriptHref_RemovesJavascript()
        {
            // Arrange
            string input = "<a href=\"javascript:alert('XSS')\">Link</a>";

            // Act
            var result = StringUtils.ScrubHtml(input);

            // Assert
            result.Should().Contain("href=\"#\"");
            result.Should().NotContain("javascript:");
        }

        [Test]
        public void ScrubHtml_WithIframe_RemovesIframe()
        {
            // Arrange
            string input = "<div>Content</div><iframe src=\"evil.com\"></iframe>";

            // Act
            var result = StringUtils.ScrubHtml(input);

            // Assert
            result.Should().NotContain("<iframe");
            result.Should().Contain("<div>Content</div>");
        }

        #endregion

        #region SanitizeUserInput Tests

        [Test]
        public void SanitizeUserInput_WithHtmlTags_RemovesTags()
        {
            // Arrange
            string input = "<p>Hello <strong>World</strong></p>";

            // Act
            var result = StringUtils.SanitizeUserInput(input);

            // Assert
            result.Should().Be("Hello World");
        }

        [Test]
        public void SanitizeUserInput_WithScriptTag_RemovesScriptCompletely()
        {
            // Arrange
            string input = "Hello<script>alert('XSS')</script>World";

            // Act
            var result = StringUtils.SanitizeUserInput(input);

            // Assert
            result.Should().Be("HelloWorld");
            result.Should().NotContain("script");
            result.Should().NotContain("XSS");
        }

        [Test]
        public void SanitizeUserInput_WithExcessiveWhitespace_CollapsesWhitespace()
        {
            // Arrange
            string input = "Hello     World\n\n\nTest";

            // Act
            var result = StringUtils.SanitizeUserInput(input);

            // Assert
            result.Should().Be("Hello World Test");
        }

        [Test]
        public void SanitizeUserInput_WithEmptyInput_ReturnsEmptyString()
        {
            // Arrange
            string input = string.Empty;

            // Act
            var result = StringUtils.SanitizeUserInput(input);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region SanitizeCommentPreservingLineBreaks Tests

        [Test]
        public void SanitizeCommentPreservingLineBreaks_WithLineBreaks_PreservesLineBreaks()
        {
            // Arrange
            string input = "Line 1\nLine 2\nLine 3";

            // Act
            var result = StringUtils.SanitizeCommentPreservingLineBreaks(input);

            // Assert
            result.Should().Contain("\n");
            result.Should().Be("Line 1\nLine 2\nLine 3");
        }

        [Test]
        public void SanitizeCommentPreservingLineBreaks_WithHtmlTags_RemovesTagsButKeepsLineBreaks()
        {
            // Arrange
            string input = "<p>Line 1</p>\n<p>Line 2</p>";

            // Act
            var result = StringUtils.SanitizeCommentPreservingLineBreaks(input);

            // Assert
            result.Should().NotContain("<p>");
            result.Should().Contain("\n");
        }

        [Test]
        public void SanitizeCommentPreservingLineBreaks_WithExcessiveLineBreaks_LimitsConsecutiveBreaks()
        {
            // Arrange
            string input = "Line 1\n\n\n\n\nLine 2";

            // Act
            var result = StringUtils.SanitizeCommentPreservingLineBreaks(input);

            // Assert
            result.Should().NotContain("\n\n\n");
            var lineBreakCount = result.Count(c => c == '\n');
            lineBreakCount.Should().BeLessOrEqualTo(2);
        }

        #endregion

        #region StripHtmlFromString Tests

        [Test]
        public void StripHtmlFromString_WithHtmlTags_RemovesTags()
        {
            // Arrange
            string input = "<div>Hello <b>World</b></div>";

            // Act
            var result = StringUtils.StripHtmlFromString(input);

            // Assert
            result.Should().Be("Hello World");
        }

        [Test]
        public void StripHtmlFromString_WithBBCode_RemovesBBCode()
        {
            // Arrange
            string input = "Hello [b]World[/b]";

            // Act
            var result = StringUtils.StripHtmlFromString(input);

            // Assert
            result.Should().Be("Hello World");
        }

        #endregion

        #region CreateUrl Tests

        [Test]
        public void CreateUrl_WithSpaces_ReplacesWithDashes()
        {
            // Arrange
            string input = "This is a Test";

            // Act
            var result = StringUtils.CreateUrl(input, "-");

            // Assert
            result.Should().Be("this-is-a-test");
        }

        [Test]
        public void CreateUrl_WithSpecialCharacters_RemovesSpecialCharacters()
        {
            // Arrange
            string input = "Test@#$Page!";

            // Act
            var result = StringUtils.CreateUrl(input, "-");

            // Assert
            result.Should().Be("test-page");
        }

        [Test]
        public void CreateUrl_WithAccentedCharacters_RemovesAccents()
        {
            // Arrange
            string input = "Café Montréal";

            // Act
            var result = StringUtils.CreateUrl(input, "-");

            // Assert
            result.Should().NotContain("é");
            result.Should().Contain("cafe");
        }

        #endregion

        #region Truncate Tests

        [Test]
        public void Truncate_WithLongerString_TruncatesToMaxLength()
        {
            // Arrange
            string input = "This is a very long string that needs truncation";

            // Act
            var result = input.Truncate(10);

            // Assert
            result.Should().HaveLength(10);
            result.Should().Be("This is a ");
        }

        [Test]
        public void Truncate_WithShorterString_ReturnsOriginalString()
        {
            // Arrange
            string input = "Short";

            // Act
            var result = input.Truncate(10);

            // Assert
            result.Should().Be("Short");
        }

        #endregion

        #region GetUniqueKey Tests

        [Test]
        public void GetUniqueKey_WithSpecifiedSize_ReturnsKeyOfCorrectLength()
        {
            // Arrange
            int size = 16;

            // Act
            var result = StringUtils.GetUniqueKey(size);

            // Assert
            result.Should().HaveLength(size);
        }

        [Test]
        public void GetUniqueKey_CalledMultipleTimes_ReturnsDifferentKeys()
        {
            // Act
            var key1 = StringUtils.GetUniqueKey(16);
            var key2 = StringUtils.GetUniqueKey(16);

            // Assert
            key1.Should().NotBe(key2);
        }

        #endregion

        #region md5HashString Tests

        [Test]
        public void md5HashString_WithSameInput_ReturnsSameHash()
        {
            // Arrange
            string input = "test@example.com";

            // Act
            var hash1 = StringUtils.md5HashString(input);
            var hash2 = StringUtils.md5HashString(input);

            // Assert
            hash1.Should().Be(hash2);
        }

        [Test]
        public void md5HashString_WithDifferentInput_ReturnsDifferentHash()
        {
            // Arrange
            string input1 = "test1@example.com";
            string input2 = "test2@example.com";

            // Act
            var hash1 = StringUtils.md5HashString(input1);
            var hash2 = StringUtils.md5HashString(input2);

            // Assert
            hash1.Should().NotBe(hash2);
        }

        #endregion
    }
}
