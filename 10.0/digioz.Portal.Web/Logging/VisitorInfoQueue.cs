using System.Threading.Channels;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Logging
{
    public sealed class VisitorInfoQueue : IVisitorInfoQueue
    {
        private readonly Channel<VisitorInfo> _channel;
        public VisitorInfoQueue()
        {
            var options = new BoundedChannelOptions(10_000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            };
            _channel = Channel.CreateBounded<VisitorInfo>(options);
            Reader = _channel.Reader;
        }

        public ChannelReader<VisitorInfo> Reader { get; }
        public bool TryEnqueue(VisitorInfo item) => _channel.Writer.TryWrite(item);
    }
}
