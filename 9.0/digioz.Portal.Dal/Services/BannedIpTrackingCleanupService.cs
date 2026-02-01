using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Dal.Services
{
    /// <summary>
    /// Service for cleaning up old IP ban tracking records.
    /// Runs periodically to prevent the BannedIpTracking table from growing indefinitely.
    /// </summary>
    public class BannedIpTrackingCleanupService : IBannedIpTrackingCleanupService
    {
        private readonly digiozPortalContext _context;
        private readonly ILogger<BannedIpTrackingCleanupService> _logger;

        public BannedIpTrackingCleanupService(
            digiozPortalContext context,
            ILogger<BannedIpTrackingCleanupService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Delete BannedIpTracking records older than the specified number of days.
        /// </summary>
        /// <param name="daysToKeep">Number of days of records to retain (default: 7)</param>
        /// <returns>Number of records deleted</returns>
        public async Task<int> CleanupOldRecordsAsync(int daysToKeep = 7)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

                var oldRecords = _context.BannedIpTrackings
                    .Where(t => t.Timestamp < cutoffDate);

                int count = await oldRecords.CountAsync();

                if (count > 0)
                {
                    _context.BannedIpTrackings.RemoveRange(oldRecords);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Cleaned up {Count} old BannedIpTracking records older than {CutoffDate}",
                        count, cutoffDate);
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old BannedIpTracking records");
                throw;
            }
        }

        /// <summary>
        /// Delete expired ban records from BannedIp table.
        /// </summary>
        /// <returns>Number of records deleted</returns>
        public async Task<int> CleanupExpiredBansAsync()
        {
            try
            {
                var now = DateTime.UtcNow;

                // Query expired bans (excluding permanent bans)
                var expiredBans = await _context.BannedIps
                    .Where(b => b.BanExpiry < now && b.BanExpiry != DateTime.MaxValue)
                    .ToListAsync();

                if (expiredBans.Any())
                {
                    _context.BannedIps.RemoveRange(expiredBans);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Cleaned up {Count} expired ban records", expiredBans.Count);
                    return expiredBans.Count;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired bans");
                return 0; // Return 0 instead of throwing to prevent service from stopping
            }
        }

        /// <summary>
        /// Get statistics about the BannedIpTracking table.
        /// </summary>
        /// <returns>Statistics object</returns>
        public async Task<BannedIpTrackingStatistics> GetStatisticsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var oneDayAgo = now.AddDays(-1);
                var oneWeekAgo = now.AddDays(-7);

                var stats = new BannedIpTrackingStatistics
                {
                    TotalRecords = await _context.BannedIpTrackings.CountAsync(),
                    RecordsLast24Hours = await _context.BannedIpTrackings
                        .Where(t => t.Timestamp > oneDayAgo)
                        .CountAsync(),
                    RecordsLastWeek = await _context.BannedIpTrackings
                        .Where(t => t.Timestamp > oneWeekAgo)
                        .CountAsync(),
                    UniqueIpsLast24Hours = await _context.BannedIpTrackings
                        .Where(t => t.Timestamp > oneDayAgo)
                        .Select(t => t.IpAddress)
                        .Distinct()
                        .CountAsync()
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting BannedIpTracking statistics");
                throw;
            }
        }
    }

    /// <summary>
    /// Statistics about the BannedIpTracking table
    /// </summary>
    public class BannedIpTrackingStatistics
    {
        public int TotalRecords { get; set; }
        public int RecordsLast24Hours { get; set; }
        public int RecordsLastWeek { get; set; }
        public int UniqueIpsLast24Hours { get; set; }
    }
}
