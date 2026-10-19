using System.Threading;
using System.Threading.Channels;
using digioz.Portal.Bo;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Services
{
    /// <summary>
    /// In-memory queue that decouples general request tracking from the request pipeline.
    /// Entries are persisted in batches by <see cref="RateLimitTrackingWriterService"/>.
    /// When the queue is full the oldest entries are dropped so the request path never
    /// blocks; drops are counted and logged so tracking loss is visible.
    /// </summary>
    public sealed class RateLimitTrackingQueue
    {
        private readonly Channel<BannedIpTracking> _channel;
        private readonly ILogger<RateLimitTrackingQueue> _logger;
        private long _droppedCount;

        public RateLimitTrackingQueue(ILogger<RateLimitTrackingQueue> logger)
        {
            _logger = logger;
            var options = new BoundedChannelOptions(10_000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            };
            _channel = Channel.CreateBounded<BannedIpTracking>(options, OnItemDropped);
        }

        public ChannelReader<BannedIpTracking> Reader => _channel.Reader;

        /// <summary>
        /// Total number of tracking records dropped because the queue was full.
        /// </summary>
        public long DroppedCount => Interlocked.Read(ref _droppedCount);

        public bool TryEnqueue(BannedIpTracking item) => _channel.Writer.TryWrite(item);

        private void OnItemDropped(BannedIpTracking dropped)
        {
            var total = Interlocked.Increment(ref _droppedCount);

            // Log the first drop and then every 1000th to avoid flooding the log under load.
            if (total == 1 || total % 1000 == 0)
            {
                _logger.LogWarning(
                    "Rate limit tracking queue full; dropped oldest record for IP: {IP}. Total dropped: {Total}",
                    dropped.IpAddress, total);
            }
        }
    }
}
