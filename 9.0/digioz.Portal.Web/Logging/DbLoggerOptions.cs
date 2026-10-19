using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Logging
{
    public sealed class DbLoggerOptions
    {
        public LogLevel MinLevel { get; set; } = LogLevel.Information;
        public bool IncludeScopes { get; set; } = false;
    }
}
