#nullable enable

using System;
using System.ComponentModel.DataAnnotations;

namespace digioz.Portal.Bo
{
    /// <summary>
    /// Tracks request counts per IP address for rate limiting purposes.
    /// Each record represents a single request from an IP address.
    /// Used to calculate rate limits across different time windows.
    /// </summary>
    public class BannedIpTracking
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The IP address making the request
        /// </summary>
        [MaxLength(64)]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// When the request was made (UTC)
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The endpoint/path that was requested
        /// </summary>
        [MaxLength(500)]
        public string RequestPath { get; set; } = string.Empty;

        /// <summary>
        /// Request type (e.g., "General", "ForgotPassword", "Login")
        /// Used for specific rate limit tracking
        /// </summary>
        [MaxLength(50)]
        public string RequestType { get; set; } = "General";

        /// <summary>
        /// Email address (for password reset tracking)
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; }

        /// <summary>
        /// User agent string
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }
    }
}

#nullable restore
