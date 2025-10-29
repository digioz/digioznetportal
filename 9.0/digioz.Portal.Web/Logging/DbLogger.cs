using System;
using System.Text;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Logging
{
    internal sealed class DbLogger : ILogger
    {
        private readonly string _category;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DbLoggerOptions _options;

        public DbLogger(string category, IServiceScopeFactory scopeFactory, DbLoggerOptions options)
        {
            _category = category;
            _scopeFactory = scopeFactory;
            _options = options;
        }

        public IDisposable BeginScope<TState>(TState state) => _options.IncludeScopes ? new NoopScope() : NoopScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _options.MinLevel && logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel) || formatter is null) return;

            try
            {
                var message = formatter(state, exception);
                if (string.IsNullOrWhiteSpace(message) && exception is null) return;

                var logEvent = $"{logLevel}|{_category}|{eventId.Id}";
                var exceptionText = exception?.ToString();

                using var scope = _scopeFactory.CreateScope();
                var logService = scope.ServiceProvider.GetRequiredService<ILogService>();

                var entity = new Log
                {
                    Message = BuildMessage(message, eventId, logLevel),
                    Exception = exceptionText,
                    LogEvent = logEvent,
                    Timestamp = DateTime.UtcNow
                };

                logService.Add(entity);
            }
            catch
            {
                // Avoid recursive logging if DB is unavailable
            }
        }

        private static string BuildMessage(string message, EventId eventId, LogLevel level)
        {
            var sb = new StringBuilder();
            sb.Append('[').Append(level).Append(']');
            if (eventId.Id != 0) sb.Append(" (EventId: ").Append(eventId.Id).Append(')');
            sb.Append(' ').Append(message);
            return sb.ToString();
        }

        private sealed class NoopScope : IDisposable
        {
            public static readonly NoopScope Instance = new NoopScope();
            public void Dispose() { }
        }
    }
}
