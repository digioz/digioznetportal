using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Logging
{
    public sealed class VisitorInfoBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly VisitorInfoQueue _queue;
        private readonly ILogger<VisitorInfoBackgroundService> _logger;

        public VisitorInfoBackgroundService(
            IServiceScopeFactory scopeFactory, 
            IVisitorInfoQueue queue,
            ILogger<VisitorInfoBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _queue = (VisitorInfoQueue)queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var reader = _queue.Reader;
            var buffer = new List<VisitorInfo>(256);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var hasItem = await reader.WaitToReadAsync(stoppingToken).ConfigureAwait(false);
                    if (!hasItem) continue;

                    buffer.Clear();
                    while (buffer.Count < 256 && reader.TryRead(out var item))
                        buffer.Add(item);

                    if (buffer.Count == 0) continue;

                    using var scope = _scopeFactory.CreateScope();
                    var svc = scope.ServiceProvider.GetRequiredService<IVisitorInfoService>();
                    svc.AddRange(buffer);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing visitor info batch");
                    await Task.Delay(500, stoppingToken);
                }
            }
        }
    }
}
