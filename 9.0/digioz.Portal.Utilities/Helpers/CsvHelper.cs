using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace digioz.Portal.Utilities.Helpers
{
    /// <summary>
    /// Utility class for CSV file operations including escaping and formatting
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// Escapes a string value for safe inclusion in a CSV file.
        /// Handles commas, quotes, newlines, and carriage returns according to RFC 4180.
        /// </summary>
        /// <param name="input">The string to escape</param>
        /// <returns>Escaped string safe for CSV inclusion</returns>
        public static string Escape(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Check if the value needs quoting
            var needsQuotes = input.Contains(',') 
                           || input.Contains('"') 
                           || input.Contains('\n') 
                           || input.Contains('\r');

            // Escape quotes by doubling them (RFC 4180)
            var value = input.Replace("\"", "\"\"");

            // Wrap in quotes if necessary
            return needsQuotes ? $"\"{value}\"" : value;
        }

        /// <summary>
        /// Builds a CSV file with headers and rows
        /// </summary>
        /// <param name="headers">CSV column headers</param>
        /// <param name="rows">Collection of row data (each row is an array of cell values)</param>
        /// <returns>Complete CSV content as string</returns>
        public static string BuildCsv(string[] headers, IEnumerable<string[]> rows)
        {
            var sb = new StringBuilder();
            
            // Add header row
            sb.AppendLine(string.Join(",", headers.Select(Escape)));
            
            // Add data rows
            foreach (var row in rows)
            {
                sb.AppendLine(string.Join(",", row.Select(Escape)));
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Converts a CSV string to UTF-8 encoded bytes suitable for file download
        /// </summary>
        /// <param name="csvContent">The CSV content string</param>
        /// <returns>UTF-8 encoded byte array</returns>
        public static byte[] ToBytes(string csvContent)
        {
            return Encoding.UTF8.GetBytes(csvContent);
        }
    }
}
