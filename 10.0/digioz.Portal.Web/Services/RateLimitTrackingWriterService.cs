using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Services
{
    /// <summary>
    /// Background writer that drains <see cref="RateLimitTrackingQueue"/> and
    /// batch-inserts BannedIpTracking rows so the request path never waits on the insert.
    /// </summary>
    public sealed class RateLimitTrackingWriterService : BackgroundService
    {
        private const int MaxBatchSize = 256;
        private const int MaxSaveAttemptsPerBatch = 3;
        private static readonly TimeSpan MaxBatchDelay = TimeSpan.FromMilliseconds(500);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RateLimitTrackingQueue _queue;
        private readonly ILogger<RateLimitTrackingWriterService> _logger;

        public RateLimitTrackingWriterService(
            IServiceScopeFactory scopeFactory,
            RateLimitTrackingQueue queue,
            ILogger<RateLimitTrackingWriterService> logger)
        {
            _scopeFactory = scopeFactory;
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var reader = _queue.Reader;
            var buffer = new List<BannedIpTracking>(MaxBatchSize);
            var saveAttempts = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Only pull new work when the previous batch was persisted (or abandoned);
                    // a failed batch is retried before reading more items.
                    if (buffer.Count == 0)
                    {
                        if (!await reader.WaitToReadAsync(stoppingToken).ConfigureAwait(false))
                        {
                            continue;
                        }

                        // Give a short window for more items to arrive so we insert in batches
                        // rather than one row per request under load.
                        await Task.Delay(MaxBatchDelay, stoppingToken).ConfigureAwait(false);

                        while (buffer.Count < MaxBatchSize && reader.TryRead(out var item))
                        {
                            buffer.Add(item);
                        }

                        if (buffer.Count == 0)
                        {
                            continue;
                        }

                        saveAttempts = 0;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<digiozPortalContext>();
                    context.BannedIpTrackings.AddRange(buffer);
                    await context.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
                    buffer.Clear();
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // shutdown
                }
                catch (Exception ex)
                {
                    saveAttempts++;
                    if (saveAttempts >= MaxSaveAttemptsPerBatch)
                    {
                        _logger.LogError(ex,
                            "Dropping {Count} rate limit tracking records after {Attempts} failed save attempts",
                            buffer.Count, saveAttempts);
                        buffer.Clear();
                    }
                    else
                    {
                        _logger.LogError(ex,
                            "Error writing {Count} rate limit tracking records (attempt {Attempt}/{Max}); will retry",
                            buffer.Count, saveAttempts, MaxSaveAttemptsPerBatch);
                    }
                    await Task.Delay(500, stoppingToken).ConfigureAwait(false);
                }
            }
        }
    }
}
