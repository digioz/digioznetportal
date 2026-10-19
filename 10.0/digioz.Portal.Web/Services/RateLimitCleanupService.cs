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
                var dalCleanupService = scope.ServiceProvider.GetRequiredService<Dal.Services.IBannedIpTrackingCleanupService>();

                // Clean old BannedIpTracking records (older than 7 days)
                var trackingRecordsRemoved = await dalCleanupService.CleanupOldRecordsAsync(7);
                if (trackingRecordsRemoved > 0)
                {
                    _logger.LogInformation("Removed {Count} old BannedIpTracking records", trackingRecordsRemoved);
                }

                // Clean expired bans
                var expiredBansRemoved = await dalCleanupService.CleanupExpiredBansAsync();
                if (expiredBansRemoved > 0)
                {
                    _logger.LogInformation("Removed {Count} expired ban records", expiredBansRemoved);
                }

                if (trackingRecordsRemoved == 0 && expiredBansRemoved == 0)
                {
                    _logger.LogDebug("No records to clean up");
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during cleanup");
                throw;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cleanup operation cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during cleanup");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Rate Limit Cleanup Service is stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}
