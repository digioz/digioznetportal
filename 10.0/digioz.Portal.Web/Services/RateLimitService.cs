using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using digioz.Portal.Bo;
using digioz.Portal.Dal;

namespace digioz.Portal.Web.Services
{
    /// <summary>
    /// Service for rate limiting using the dedicated BannedIpTracking table.
    /// Handles request tracking and rate limit checks.
    /// </summary>
    public class RateLimitService
    {
        private readonly digiozPortalContext _context;
        private readonly ILogger<RateLimitService> _logger;

        public RateLimitService(
            digiozPortalContext context,
            ILogger<RateLimitService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Track a request in the BannedIpTracking table
        /// </summary>
        public async Task TrackRequestAsync(string ipAddress, string path, string requestType = "General", string? email = null, string? userAgent = null)
        {
            try
            {
                var tracking = new BannedIpTracking
                {
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    RequestPath = path,
                    RequestType = requestType,
                    Email = email,
                    UserAgent = userAgent
                };

                _context.BannedIpTrackings.Add(tracking);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking request for IP: {IP}, Path: {Path}", ipAddress, path);
                // Don't throw - tracking failures shouldn't block requests
            }
        }

        /// <summary>
        /// Check if an IP address has exceeded the rate limit
        /// </summary>
        public async Task<bool> CheckRateLimitAsync(string ipAddress, int maxRequests, int windowMinutes)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return true; // Allow if IP is invalid
            }

            try
            {
                var windowStart = DateTime.UtcNow.AddMinutes(-windowMinutes);

                var count = await _context.BannedIpTrackings
                    .Where(t => t.IpAddress == ipAddress && t.Timestamp > windowStart)
                    .CountAsync();

                if (count >= maxRequests)
                {
                    _logger.LogWarning("Rate limit exceeded - IP: {IP}, Count: {Count}/{Max}, Window: {Window}min",
                        ipAddress, count, maxRequests, windowMinutes);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rate limit for IP: {IP}", ipAddress);
                return true; // Fail open
            }
        }

        /// <summary>
        /// Check password reset rate limit per email address
        /// </summary>
        public async Task<bool> CheckPasswordResetLimitPerEmailAsync(string email, int maxAttempts, int windowHours)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return true;
            }

            try
            {
                var windowStart = DateTime.UtcNow.AddHours(-windowHours);
                var normalizedEmail = email.ToLowerInvariant();

                var count = await _context.BannedIpTrackings
                    .Where(t => t.Email == normalizedEmail &&
                               t.RequestType == "ForgotPassword" &&
                               t.Timestamp > windowStart)
                    .CountAsync();

                if (count >= maxAttempts)
                {
                    _logger.LogWarning("Password reset limit exceeded (email) - Email: {Email}, Count: {Count}/{Max}",
                        normalizedEmail, count, maxAttempts);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password reset limit for email: {Email}", email);
                return true; // Fail open
            }
        }

        /// <summary>
        /// Check password reset rate limit per IP address
        /// </summary>
        public async Task<bool> CheckPasswordResetLimitPerIpAsync(string ipAddress, int maxAttempts, int windowHours)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return true;
            }

            try
            {
                var windowStart = DateTime.UtcNow.AddHours(-windowHours);

                var count = await _context.BannedIpTrackings
                    .Where(t => t.IpAddress == ipAddress &&
                               t.RequestType == "ForgotPassword" &&
                               t.Timestamp > windowStart)
                    .CountAsync();

                if (count >= maxAttempts)
                {
                    _logger.LogWarning("Password reset limit exceeded (IP) - IP: {IP}, Count: {Count}/{Max}",
                        ipAddress, count, maxAttempts);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password reset limit for IP: {IP}", ipAddress);
                return true; // Fail open
            }
        }

        /// <summary>
        /// Check login rate limit per email address
        /// </summary>
        public async Task<bool> CheckLoginLimitPerEmailAsync(string email, int maxAttempts, int windowHours)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return true;
            }

            try
            {
                var windowStart = DateTime.UtcNow.AddHours(-windowHours);
                var normalizedEmail = email.ToLowerInvariant();

                var count = await _context.BannedIpTrackings
                    .Where(t => t.Email == normalizedEmail &&
                               t.RequestType == "Login" &&
                               t.Timestamp > windowStart)
                    .CountAsync();

                if (count >= maxAttempts)
                {
                    _logger.LogWarning("Login limit exceeded (email) - Email: {Email}, Count: {Count}/{Max}",
                        normalizedEmail, count, maxAttempts);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking login limit for email: {Email}", email);
                return true; // Fail open
            }
        }

        /// <summary>
        /// Check login rate limit per IP address
        /// </summary>
        public async Task<bool> CheckLoginLimitPerIpAsync(string ipAddress, int maxAttempts, int windowHours)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return true;
            }

            try
            {
                var windowStart = DateTime.UtcNow.AddHours(-windowHours);

                var count = await _context.BannedIpTrackings
                    .Where(t => t.IpAddress == ipAddress &&
                               t.RequestType == "Login" &&
                               t.Timestamp > windowStart)
                    .CountAsync();

                if (count >= maxAttempts)
                {
                    _logger.LogWarning("Login limit exceeded (IP) - IP: {IP}, Count: {Count}/{Max}",
                        ipAddress, count, maxAttempts);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking login limit for IP: {IP}", ipAddress);
                return true; // Fail open
            }
        }

        /// <summary>
        /// Check registration rate limit per email address
        /// </summary>
        public async Task<bool> CheckRegistrationLimitPerEmailAsync(string email, int maxAttempts, int windowHours)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return true;
            }

            try
            {
                var windowStart = DateTime.UtcNow.AddHours(-windowHours);
                var normalizedEmail = email.ToLowerInvariant();

                var count = await _context.BannedIpTrackings
                    .Where(t => t.Email == normalizedEmail &&
                               t.RequestType == "Registration" &&
                               t.Timestamp > windowStart)
                    .CountAsync();

                if (count >= maxAttempts)
                {
                    _logger.LogWarning("Registration limit exceeded (email) - Email: {Email}, Count: {Count}/{Max}",
                        normalizedEmail, count, maxAttempts);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking registration limit for email: {Email}", email);
                return true; // Fail open
            }
        }

        /// <summary>
        /// Check registration rate limit per IP address
        /// </summary>
        public async Task<bool> CheckRegistrationLimitPerIpAsync(string ipAddress, int maxAttempts, int windowHours)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return true;
            }

            try
            {
                var windowStart = DateTime.UtcNow.AddHours(-windowHours);

                var count = await _context.BannedIpTrackings
                    .Where(t => t.IpAddress == ipAddress &&
                               t.RequestType == "Registration" &&
                               t.Timestamp > windowStart)
                    .CountAsync();

                if (count >= maxAttempts)
                {
                    _logger.LogWarning("Registration limit exceeded (IP) - IP: {IP}, Count: {Count}/{Max}",
                        ipAddress, count, maxAttempts);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking registration limit for IP: {IP}", ipAddress);
                return true; // Fail open
            }
        }

        /// <summary>
        /// Get request count for an IP address (for admin/debugging)
        /// </summary>
        public async Task<int> GetRequestCountAsync(string ipAddress, int windowMinutes)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return 0;
            }

            try
            {
                var windowStart = DateTime.UtcNow.AddMinutes(-windowMinutes);

                return await _context.BannedIpTrackings
                    .Where(t => t.IpAddress == ipAddress && t.Timestamp > windowStart)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting request count for IP: {IP}", ipAddress);
                return 0;
            }
        }

        /// <summary>
        /// Get detailed rate limit info for an IP address (for admin dashboards)
        /// </summary>
        public async Task<RateLimitInfo> GetRateLimitInfoAsync(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return new RateLimitInfo { IpAddress = ipAddress };
            }

            try
            {
                var now = DateTime.UtcNow;
                var oneMinuteAgo = now.AddMinutes(-1);
                var tenMinutesAgo = now.AddMinutes(-10);
                var oneHourAgo = now.AddHours(-1);

                var recentRequests = await _context.BannedIpTrackings
                    .Where(t => t.IpAddress == ipAddress && t.Timestamp > oneHourAgo)
                    .Select(t => t.Timestamp)
                    .ToListAsync();

                return new RateLimitInfo
                {
                    IpAddress = ipAddress,
                    RequestsLastMinute = recentRequests.Count(t => t > oneMinuteAgo),
                    RequestsLast10Minutes = recentRequests.Count(t => t > tenMinutesAgo),
                    RequestsLastHour = recentRequests.Count,
                    LastRequestTime = recentRequests.Any() ? recentRequests.Max() : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rate limit info for IP: {IP}", ipAddress);
                return new RateLimitInfo { IpAddress = ipAddress };
            }
        }
    }

    /// <summary>
    /// Rate limit information for an IP address
    /// </summary>
    public class RateLimitInfo
    {
        public string IpAddress { get; set; } = string.Empty;
        public int RequestsLastMinute { get; set; }
        public int RequestsLast10Minutes { get; set; }
        public int RequestsLastHour { get; set; }
        public DateTime? LastRequestTime { get; set; }
    }
}
