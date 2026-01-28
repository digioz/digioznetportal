using System;

namespace digioz.Portal.Bo
{
    /// <summary>
    /// Represents a banned IP address with expiry and tracking information.
    /// Used for persistent bot/attack protection across server restarts.
    /// </summary>
    public class BannedIp
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The banned IP address
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// When the ban expires (DateTime.MaxValue for permanent bans)
        /// </summary>
        public DateTime BanExpiry { get; set; }

        /// <summary>
        /// Reason for the ban (rate limit exceeded, bot activity, etc.)
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Number of times this IP has been banned (for escalating to permanent bans)
        /// </summary>
        public int BanCount { get; set; }

        /// <summary>
        /// When the ban was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// User agent string of the banned request (empty string if not available)
        /// </summary>
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>
        /// Email address that was being targeted (empty string if not applicable)
        /// </summary>
        public string AttemptedEmail { get; set; } = string.Empty;

        /// <summary>
        /// Check if this ban is still active
        /// </summary>
        public bool IsActive => DateTime.UtcNow < BanExpiry;

        /// <summary>
        /// Check if this is a permanent ban
        /// </summary>
        public bool IsPermanent => BanExpiry == DateTime.MaxValue;
    }
}
