using System.Threading.Tasks;

namespace digioz.Portal.Dal.Services
{
    /// <summary>
    /// Interface for IP ban tracking cleanup operations
    /// </summary>
    public interface IBannedIpTrackingCleanupService
    {
        /// <summary>
        /// Delete BannedIpTracking records older than the specified number of days
        /// </summary>
        /// <param name="daysToKeep">Number of days of records to retain</param>
        /// <returns>Number of records deleted</returns>
        Task<int> CleanupOldRecordsAsync(int daysToKeep = 7);

        /// <summary>
        /// Delete expired ban records from BannedIp table
        /// </summary>
        /// <returns>Number of records deleted</returns>
        Task<int> CleanupExpiredBansAsync();

        /// <summary>
        /// Get statistics about the BannedIpTracking table
        /// </summary>
        /// <returns>Statistics object</returns>
        Task<BannedIpTrackingStatistics> GetStatisticsAsync();
    }
}
