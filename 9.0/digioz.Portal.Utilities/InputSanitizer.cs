using System;
using System.Collections.Generic;
using System.Linq;
using Ganss.Xss;

namespace digioz.Portal.Utilities
{
    /// <summary>
    /// Provides input validation and sanitization utilities for user-generated content
    /// </summary>
    public static class InputSanitizer
    {
        private static readonly HtmlSanitizer _htmlSanitizer = new HtmlSanitizer();

        static InputSanitizer()
        {
            // Configure the sanitizer to strip all HTML tags by default
            _htmlSanitizer.AllowedTags.Clear();
            _htmlSanitizer.AllowedAttributes.Clear();
            _htmlSanitizer.AllowedCssProperties.Clear();
            _htmlSanitizer.AllowedSchemes.Clear();
        }

        /// <summary>
        /// Sanitizes input by removing HTML tags and trimming whitespace
        /// </summary>
        /// <param name="input">The input string to sanitize</param>
        /// <param name="maxLength">Optional maximum length (0 = no limit)</param>
        /// <returns>Sanitized string</returns>
        public static string SanitizeText(string? input, int maxLength = 0)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove HTML tags
            var sanitized = _htmlSanitizer.Sanitize(input).Trim();

            // Apply length limit if specified
            if (maxLength > 0 && sanitized.Length > maxLength)
            {
                sanitized = sanitized.Substring(0, maxLength);
            }

            return sanitized;
        }

        /// <summary>
        /// Sanitizes a collection of strings (e.g., poll answers)
        /// </summary>
        /// <param name="inputs">Collection of input strings</param>
        /// <param name="maxLength">Maximum length per item</param>
        /// <param name="minCount">Minimum number of valid items required</param>
        /// <param name="maxCount">Maximum number of items allowed</param>
        /// <returns>Sanitized list of strings</returns>
        public static List<string> SanitizeList(IEnumerable<string>? inputs, int maxLength = 0, int minCount = 0, int maxCount = 0)
        {
            if (inputs == null)
                return new List<string>();

            var sanitized = inputs
                .Select(i => SanitizeText(i, maxLength))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Apply max count if specified
            if (maxCount > 0 && sanitized.Count > maxCount)
            {
                sanitized = sanitized.Take(maxCount).ToList();
            }

            return sanitized;
        }

        /// <summary>
        /// Validates that a string is not empty and within length limits
        /// </summary>
        /// <param name="input">Input to validate</param>
        /// <param name="fieldName">Name of the field for error messages</param>
        /// <param name="minLength">Minimum required length</param>
        /// <param name="maxLength">Maximum allowed length</param>
        /// <returns>Error message if invalid, null if valid</returns>
        public static string? ValidateString(string? input, string fieldName, int minLength = 1, int maxLength = 500)
        {
            if (string.IsNullOrWhiteSpace(input))
                return $"{fieldName} is required.";

            var trimmed = input.Trim();
            
            if (trimmed.Length < minLength)
                return $"{fieldName} must be at least {minLength} characters.";

            if (trimmed.Length > maxLength)
                return $"{fieldName} must not exceed {maxLength} characters.";

            return null;
        }

        /// <summary>
        /// Validates a list of items (e.g., poll answers)
        /// </summary>
        /// <param name="items">List of items to validate</param>
        /// <param name="fieldName">Name of the field for error messages</param>
        /// <param name="minCount">Minimum number of items required</param>
        /// <param name="maxCount">Maximum number of items allowed</param>
        /// <returns>Error message if invalid, null if valid</returns>
        public static string? ValidateList(IEnumerable<string>? items, string fieldName, int minCount = 2, int maxCount = 50)
        {
            if (items == null || !items.Any())
                return $"At least {minCount} {fieldName} are required.";

            var validItems = items.Where(i => !string.IsNullOrWhiteSpace(i)).ToList();

            if (validItems.Count < minCount)
                return $"At least {minCount} {fieldName} are required.";

            if (validItems.Count > maxCount)
                return $"No more than {maxCount} {fieldName} are allowed.";

            return null;
        }

        /// <summary>
        /// Sanitizes poll question text
        /// </summary>
        public static string SanitizePollQuestion(string? question)
        {
            return SanitizeText(question, maxLength: 500);
        }

        /// <summary>
        /// Sanitizes poll answer text
        /// </summary>
        public static string SanitizePollAnswer(string? answer)
        {
            return SanitizeText(answer, maxLength: 200);
        }

        /// <summary>
        /// Sanitizes a collection of poll answers
        /// </summary>
        public static List<string> SanitizePollAnswers(IEnumerable<string>? answers)
        {
            return SanitizeList(answers, maxLength: 200, minCount: 2, maxCount: 50);
        }
    }
}
