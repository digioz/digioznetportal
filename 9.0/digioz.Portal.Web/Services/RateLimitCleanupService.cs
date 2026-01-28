using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using digioz.Portal.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace digioz.Portal.Web.Services
{
    /// <summary>
    /// Background service that periodically cleans up expired rate limit tracking data
    /// and expired ban records from the database.
    /// </summary>
    public class RateLimitCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RateLimitCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

        public RateLimitCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<RateLimitCleanupService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Rate Limit Cleanup Service starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_cleanupInterval, stoppingToken);

                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await PerformCleanupAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Expected when the service is stopping
                    _logger.LogInformation("Rate Limit Cleanup Service stopping due to cancellation");
                    break;
                }
                catch (Exception ex)
                {
                    // Log the error and continue - don't let one failure stop the service
                    _logger.LogError(ex, "Error occurred during rate limit cleanup cycle. Will retry in {Interval}", _cleanupInterval);
                }
            }

            _logger.LogInformation("Rate Limit Cleanup Service stopped");
        }

        /// <summary>
        /// Performs cleanup of expired ban records from the database
        /// </summary>
        private async Task PerformCleanupAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Starting rate limit cleanup cycle");

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var banService = scope.ServiceProvider.GetRequiredService<BanManagementService>();

                var now = DateTime.UtcNow;

                // Clean expired bans from database
                var expiredBans = await dbContext.BannedIp
                    .Where(b => b.BanExpiry < now && b.BanExpiry != DateTime.MaxValue)
                    .ToListAsync(cancellationToken);

                if (expiredBans.Any())
                {
                    _logger.LogInformation("Removing {Count} expired ban records from database", expiredBans.Count);
                    
                    dbContext.BannedIp.RemoveRange(expiredBans);
                    var removedCount = await dbContext.SaveChangesAsync(cancellationToken);
                    
                    _logger.LogInformation("Successfully removed {Count} expired ban records", removedCount);
                }
                else
                {
                    _logger.LogDebug("No expired ban records to clean up");
                }

                // Clean expired entries from the ban management service cache
                var cacheCleanedCount = banService.ClearExpiredFromCache();
                
                if (cacheCleanedCount > 0)
                {
                    _logger.LogInformation("Cleared {Count} expired bans from in-memory cache", cacheCleanedCount);
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during ban cleanup. Error: {Message}", ex.Message);
                throw; // Re-throw to be caught by ExecuteAsync's outer try-catch
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cleanup operation cancelled");
                throw; // Re-throw to stop the service
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during ban cleanup: {Message}", ex.Message);
                throw; // Re-throw to be caught by ExecuteAsync's outer try-catch
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Rate Limit Cleanup Service is stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}
