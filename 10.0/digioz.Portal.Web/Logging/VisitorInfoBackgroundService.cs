using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace digioz.Portal.Web.Logging
{
    public sealed class VisitorInfoBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly VisitorInfoQueue _queue;

        public VisitorInfoBackgroundService(IServiceScopeFactory scopeFactory, IVisitorInfoQueue queue)
        {
            _scopeFactory = scopeFactory;
            _queue = (VisitorInfoQueue)queue;
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
                catch
                {
                    await Task.Delay(500, stoppingToken);
                }
            }
        }
    }
}
