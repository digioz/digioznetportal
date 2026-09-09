using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace digioz.Portal.Web.Logging
{
    public sealed class DbLoggerProvider : ILoggerProvider
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DbLoggerOptions _options;

        public DbLoggerProvider(IServiceScopeFactory scopeFactory, IOptions<DbLoggerOptions> options)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _options = options?.Value ?? new DbLoggerOptions();
        }

        public ILogger CreateLogger(string categoryName) => new DbLogger(categoryName, _scopeFactory, _options);

        public void Dispose() { }
    }
}
