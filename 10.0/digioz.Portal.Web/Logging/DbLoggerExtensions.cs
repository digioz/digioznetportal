using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Logging
{
    public static class DbLoggerExtensions
    {
        public static ILoggingBuilder AddDbLogger(this ILoggingBuilder builder, Action<DbLoggerOptions>? configure = null)
        {
            if (configure != null)
                builder.Services.Configure(configure);

            builder.Services.AddSingleton<ILoggerProvider, DbLoggerProvider>();
            return builder;
        }
    }
}
