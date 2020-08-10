using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace digioz.Portal.Web.Helpers
{
    /// <summary>
    /// Methods to remove HTML from strings.
    /// </summary>
    public static class HtmlRemoval
    {
        /// <summary>
        /// Remove HTML from string with Regex.
        /// </summary>
        public static string StripTagsRegex(string source)
        {
            string result = Regex.Replace(source, "<.*?>", string.Empty);
            result = result.Replace("&nbsp;", " ");
            result = result.Trim();

            return result;
        }

        /// <summary>
        /// Compiled regular expression for performance.
        /// </summary>
        static Regex _htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);

        /// <summary>
        /// Remove HTML from string with compiled Regex.
        /// </summary>
        public static string StripTagsRegexCompiled(string source)
        {
            return _htmlRegex.Replace(source, string.Empty);
        }

        /// <summary>
        /// Remove HTML tags from string using char array.
        /// </summary>
        public static string StripTagsCharArray(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        /// <summary>
        /// Trim a string to a maximum value
        /// </summary>
        public static string Truncate(this string value, int maxLength)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
            {
                return value.Substring(0, maxLength);
            }

            return value;
        }

        /// <summary>
        /// Trim a string to a maximum value and add ...
        /// </summary>
        public static string TruncateDotDotDot(this string value, int maxLength)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
            {
                return value.Substring(0, maxLength) + "...";
            }

            return value;
        }
    }
}