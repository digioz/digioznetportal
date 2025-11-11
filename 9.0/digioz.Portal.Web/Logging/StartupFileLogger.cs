using System;
using System.IO;

namespace digioz.Portal.Web.Logging
{
    /// <summary>
    /// Provides file-based logging for critical startup errors when the database is unavailable.
    /// This logger writes directly to the file system and does not depend on any database services.
    /// </summary>
    public static class StartupFileLogger
    {
        private static readonly string LogDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
        private static readonly string LogFile = Path.Combine(LogDirectory, $"startup-errors-{DateTime.Now:yyyy-MM-dd}.log");

        /// <summary>
        /// Logs a critical error with exception details to a file.
        /// This method swallows all exceptions to prevent application crashes during logging.
        /// </summary>
        /// <param name="ex">The exception to log</param>
        /// <param name="message">A descriptive message about the error</param>
        public static void LogCritical(Exception ex, string message)
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] CRITICAL: {message}{Environment.NewLine}{ex}{Environment.NewLine}{new string('-', 80)}{Environment.NewLine}";
                File.AppendAllText(LogFile, logEntry);
            }
            catch
            {
                // Last resort: swallow to prevent app crash during logging
                // Consider writing to Windows Event Log here as ultimate fallback
            }
        }
    }
}
