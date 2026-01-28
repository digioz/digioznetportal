using System;
using System.Collections.Concurrent;
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
    /// Centralized service for managing IP bans with both in-memory cache and database persistence.
    /// Used by both RateLimitingMiddleware and Admin UI to ensure consistency.
    /// </summary>
    public class BanManagementService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BanManagementService> _logger;
        
        // Shared in-memory cache for fast ban lookups (shared with middleware)
        private static readonly ConcurrentDictionary<string, BanInfo> _bannedIpsCache = new();
        
        public BanManagementService(
            IServiceScopeFactory scopeFactory,
            ILogger<BanManagementService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        
        /// <summary>
        /// Check if an IP is currently banned (checks cache first, then database)
        /// </summary>
        public async Task<(bool IsBanned, BanInfo? BanInfo)> IsBannedAsync(string ipAddress)
        {
            // Check memory cache first
            if (_bannedIpsCache.TryGetValue(ipAddress, out var banInfo))
            {
                if (banInfo.IsPermanent || DateTime.UtcNow < banInfo.BanExpiry)
                {
                    return (true, banInfo);
                }
                
                // Ban expired in cache, remove it
                _bannedIpsCache.TryRemove(ipAddress, out _);
            }
            
            // Check database
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var dbBan = await dbContext.BannedIp
                .Where(b => b.IpAddress == ipAddress)
                .OrderByDescending(b => b.CreatedDate)
                .FirstOrDefaultAsync();
            
            if (dbBan != null && dbBan.IsActive)
            {
                // Add to cache for faster future lookups
                banInfo = new BanInfo
                {
                    BanExpiry = dbBan.BanExpiry,
                    IsPermanent = dbBan.IsPermanent,
                    Reason = dbBan.Reason
                };
                _bannedIpsCache.TryAdd(ipAddress, banInfo);
                return (true, banInfo);
            }
            
            return (false, null);
        }
        
        /// <summary>
        /// Ban an IP address (adds to both cache and database)
        /// </summary>
        public async Task BanIpAsync(
            string ipAddress, 
            string reason, 
            DateTime banExpiry,
            int banCount = 1,
            string userAgent = "",
            string attemptedEmail = "")
        {
            bool isPermanent = banExpiry == DateTime.MaxValue;
            
            // Add to memory cache immediately
            var banInfo = new BanInfo
            {
                BanExpiry = banExpiry,
                IsPermanent = isPermanent,
                Reason = reason
            };
            _bannedIpsCache.AddOrUpdate(ipAddress, banInfo, (key, oldValue) => banInfo);
            
            // Add to database for persistence
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
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
                
                dbContext.BannedIp.Add(bannedIp);
                await dbContext.SaveChangesAsync();
                
                _logger.LogWarning(
                    isPermanent 
                        ? "PERMANENT BAN: IP {IpAddress} banned permanently. Reason: {Reason}" 
                        : "TEMPORARY BAN: IP {IpAddress} banned until {Expiry}. Reason: {Reason}",
                    ipAddress, banExpiry, reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save banned IP {IpAddress} to database", ipAddress);
                throw;
            }
        }
        
        /// <summary>
        /// Unban an IP address (removes from both cache and database)
        /// </summary>
        public async Task UnbanIpAsync(string ipAddress)
        {
            // Remove from cache
            _bannedIpsCache.TryRemove(ipAddress, out _);
            
            // Remove from database
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var bans = await dbContext.BannedIp
                .Where(b => b.IpAddress == ipAddress)
                .ToListAsync();
            
            if (bans.Any())
            {
                dbContext.BannedIp.RemoveRange(bans);
                await dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Unbanned IP: {IpAddress}", ipAddress);
            }
        }
        
        /// <summary>
        /// Get cache statistics (for monitoring/debugging)
        /// </summary>
        public (int TotalCached, int ActiveBans, int ExpiredBans) GetCacheStatistics()
        {
            var now = DateTime.UtcNow;
            var total = _bannedIpsCache.Count;
            var active = _bannedIpsCache.Count(kvp => 
                kvp.Value.IsPermanent || kvp.Value.BanExpiry > now);
            var expired = total - active;
            
            return (total, active, expired);
        }
        
        /// <summary>
        /// Clear expired bans from cache
        /// </summary>
        public int ClearExpiredFromCache()
        {
            var now = DateTime.UtcNow;
            var expired = _bannedIpsCache
                .Where(kvp => !kvp.Value.IsPermanent && kvp.Value.BanExpiry < now)
                .Select(kvp => kvp.Key)
                .ToList();
            
            foreach (var ip in expired)
            {
                _bannedIpsCache.TryRemove(ip, out _);
            }
            
            _logger.LogInformation("Cleared {Count} expired bans from cache", expired.Count);
            return expired.Count;
        }
        
        /// <summary>
        /// Ban info for caching
        /// </summary>
        public class BanInfo
        {
            public DateTime BanExpiry { get; set; }
            public bool IsPermanent { get; set; }
            public string Reason { get; set; } = string.Empty;
        }
    }
}
