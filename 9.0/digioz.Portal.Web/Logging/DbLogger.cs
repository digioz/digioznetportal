using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Logging
{
    internal sealed class DbLogger : ILogger, IDisposable
    {
        private static readonly ConcurrentQueue<Log> _queue = new();
        private static readonly SemaphoreSlim _drainLock = new(1, 1);
        private static readonly TimeSpan _flushInterval = TimeSpan.FromMilliseconds(500);
        private static DateTime _lastFlush = DateTime.UtcNow;

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

                var entity = new Log
                {
                    Message = BuildMessage(message, eventId, logLevel),
                    Exception = exceptionText,
                    LogEvent = logEvent,
                    Timestamp = DateTime.UtcNow
                };

                // Enqueue log for batch writing
                _queue.Enqueue(entity);

                // Try opportunistic flush on interval or when queue grows
                if (_queue.Count >= 50 || (DateTime.UtcNow - _lastFlush) >= _flushInterval)
                {
                    _ = FlushAsync();
                }
            }
            catch
            {
                // Avoid recursive logging if something goes wrong
            }
        }

        private async System.Threading.Tasks.Task FlushAsync()
        {
            if (!await _drainLock.WaitAsync(0)) return; // someone else is flushing
            try
            {
                var buffer = new List<Log>(256);
                while (buffer.Count < 256 && _queue.TryDequeue(out var item))
                {
                    buffer.Add(item);
                }

                if (buffer.Count == 0) return;

                using var scope = _scopeFactory.CreateScope();
                var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
                // batch write
                if (logService is not null)
                {
                    if (buffer.Count == 1)
                        logService.Add(buffer[0]);
                    else
                        logService.AddRange(buffer);
                }
            }
            catch
            {
                // swallow all to not break app logging path
            }
            finally
            {
                _lastFlush = DateTime.UtcNow;
                _drainLock.Release();
            }
        }

        public void Dispose()
        {
            try { FlushAsync().GetAwaiter().GetResult(); } catch { }
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
