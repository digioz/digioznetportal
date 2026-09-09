using System.Threading.Channels;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Services
{
    /// <summary>
    /// In-memory queue that decouples general request tracking from the request pipeline.
    /// Entries are persisted in batches by <see cref="RateLimitTrackingWriterService"/>.
    /// </summary>
    public sealed class RateLimitTrackingQueue
    {
        private readonly Channel<BannedIpTracking> _channel;

        public RateLimitTrackingQueue()
        {
            var options = new BoundedChannelOptions(10_000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            };
            _channel = Channel.CreateBounded<BannedIpTracking>(options);
        }

        public ChannelReader<BannedIpTracking> Reader => _channel.Reader;

        public bool TryEnqueue(BannedIpTracking item) => _channel.Writer.TryWrite(item);
    }
}
