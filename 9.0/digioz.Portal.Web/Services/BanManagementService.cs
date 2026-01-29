using System;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Services
{
    /// <summary>
    /// Service for managing IP bans (database-only, no caching).
    /// Used by both RateLimitingMiddleware and Admin UI.
    /// </summary>
    public class BanManagementService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BanManagementService> _logger;
        
        public BanManagementService(
            IServiceScopeFactory scopeFactory,
            ILogger<BanManagementService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        
        /// <summary>
        /// Check if an IP is currently banned
        /// </summary>
        public async Task<(bool IsBanned, BanInfo? BanInfo)> IsBannedAsync(string ipAddress)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Dal.digiozPortalContext>();
            
            var now = DateTime.UtcNow;
            
            // Use database column BanExpiry instead of computed property IsActive
            // This allows EF Core to translate the query to SQL
            var dbBan = await dbContext.BannedIps
                .Where(b => b.IpAddress == ipAddress && b.BanExpiry > now)
                .OrderByDescending(b => b.CreatedDate)
                .FirstOrDefaultAsync();
            
            if (dbBan != null)
            {
                var banInfo = new BanInfo
                {
                    BanExpiry = dbBan.BanExpiry,
                    IsPermanent = dbBan.BanExpiry == DateTime.MaxValue,
                    Reason = dbBan.Reason
                };
                return (true, banInfo);
            }
            
            return (false, null);
        }
        
        /// <summary>
        /// Ban an IP address
        /// </summary>
        public async Task BanIpAsync(
            string ipAddress, 
            string reason, 
            DateTime banExpiry,
            int banCount = 1,
            string userAgent = "",
            string attemptedEmail = "")
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Dal.digiozPortalContext>();
            
            try
            {
                var bannedIp = new BannedIp
                {
                    IpAddress = ipAddress,
                    BanExpiry = banExpiry,
                    Reason = reason,
                    BanCount = banCount,
                    CreatedDate = DateTime.UtcNow,
                    UserAgent = userAgent ?? string.Empty,
                    AttemptedEmail = attemptedEmail ?? string.Empty
                };
                
                dbContext.BannedIps.Add(bannedIp);
                await dbContext.SaveChangesAsync();
                
                bool isPermanent = banExpiry == DateTime.MaxValue;
                _logger.LogWarning("IP banned - IP: {IP}, Reason: {Reason}, Expires: {Expiry}, Count: {Count}",
                    ipAddress, reason, isPermanent ? "Permanent" : banExpiry.ToString("yyyy-MM-dd HH:mm"), banCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ban IP: {IP}", ipAddress);
                throw;
            }
        }
        
        /// <summary>
        /// Unban an IP address and remove all associated tracking records
        /// </summary>
        public async Task UnbanIpAsync(string ipAddress)
        {
            using var scope = _scopeFactory.CreateScope();
            var dalContext = scope.ServiceProvider.GetRequiredService<Dal.digiozPortalContext>();
            
            // Remove ban records
            var bans = await dalContext.BannedIps
                .Where(b => b.IpAddress == ipAddress)
                .ToListAsync();
            
            // Remove tracking records (cascade delete)
            var trackingRecords = await dalContext.BannedIpTrackings
                .Where(t => t.IpAddress == ipAddress)
                .ToListAsync();
            
            if (bans.Any() || trackingRecords.Any())
            {
                if (bans.Any())
                {
                    dalContext.BannedIps.RemoveRange(bans);
                    _logger.LogInformation("IP unbanned: {IP} - Removing {Count} ban record(s)", ipAddress, bans.Count);
                }
                
                if (trackingRecords.Any())
                {
                    dalContext.BannedIpTrackings.RemoveRange(trackingRecords);
                    _logger.LogInformation("Removing {Count} tracking record(s) for unbanned IP: {IP}", trackingRecords.Count, ipAddress);
                }

                await dalContext.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("No ban or tracking records found for IP: {IP}", ipAddress);
            }
        }
        
        /// <summary>
        /// Ban info
        /// </summary>
        public class BanInfo
        {
            public DateTime BanExpiry { get; set; }
            public bool IsPermanent { get; set; }
            public string Reason { get; set; } = string.Empty;
        }
    }
}
