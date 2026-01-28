using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using digioz.Portal.Utilities.Helpers;
using digioz.Portal.Utilities;
using digioz.Portal.Web.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using digioz.Portal.Web.Services;

namespace digioz.Portal.Web.Middleware
{
    /// <summary>
    /// Middleware that implements IP-based rate limiting with automatic ban enforcement.
    /// Tracks requests per IP address and blocks excessive requests from bots or attackers.
    /// Uses both in-memory tracking (for performance) and database (for persistence).
    /// Cleanup of expired data is handled by RateLimitCleanupService.
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        
        // In-memory concurrent dictionaries for fast request tracking
        private static readonly ConcurrentDictionary<string, RequestTracker> _requestTracking = new();

        public RateLimitingMiddleware(
            RequestDelegate next, 
            ILogger<RateLimitingMiddleware> logger,
            IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get client IP address
            var ipAddress = IpAddressHelper.GetUserIPAddress(context);

            // Check if rate limiting is enabled via Plugin configuration
            using var scope = _scopeFactory.CreateScope();
            var configService = scope.ServiceProvider.GetRequiredService<Dal.Services.Interfaces.IConfigService>();
            var pluginService = scope.ServiceProvider.GetRequiredService<Dal.Services.Interfaces.IPluginService>();
            var configLogger = scope.ServiceProvider.GetRequiredService<ILogger<RateLimitConfiguration>>();
            
            var config = new RateLimitConfiguration(configService, pluginService, configLogger);
            
            if (!config.IsEnabled)
            {
                // Rate limiting is disabled, skip all checks
                await _next(context);
                return;
            }
            
            // Check if IP is banned (check memory cache first, then database)
            using var banScope = _scopeFactory.CreateScope();
            var banService = banScope.ServiceProvider.GetRequiredService<BanManagementService>();
            var banCheckResult = await banService.IsBannedAsync(ipAddress);
            
            if (banCheckResult.IsBanned && banCheckResult.BanInfo != null)
            {
                _logger.LogWarning("Blocked request from banned IP: {IpAddress}. Ban expires: {Expiry}. Reason: {Reason}", 
                    ipAddress, 
                    banCheckResult.BanInfo.IsPermanent ? "Permanent" : banCheckResult.BanInfo.BanExpiry.ToString(), 
                    banCheckResult.BanInfo.Reason);
                    
                context.Response.StatusCode = 429; // Too Many Requests
                context.Response.Headers.Append("Retry-After", "3600"); // Retry after 1 hour
                await context.Response.WriteAsync("Too many requests. Your IP has been temporarily blocked.");
                return;
            }
            
            // Check for bot and rate limit
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var isBot = BotHelper.IsBot(userAgent);
            
            if (isBot)
            {
                var botName = BotHelper.ExtractBotName(userAgent);
                
                // Allow legitimate crawlers but with stricter limits
                if (IsLegitimateBot(botName))
                {
                    if (!CheckRateLimit(ipAddress, config.MaxRequestsPerMinute / 2, 1)) // Half the normal limit
                    {
                        _logger.LogWarning("Rate limit exceeded for legitimate bot: {BotName} from IP: {IpAddress}", 
                            botName, ipAddress);
                        context.Response.StatusCode = 429;
                        await context.Response.WriteAsync("Rate limit exceeded for crawlers.");
                        return;
                    }
                }
                else
                {
                    // Block unknown/malicious bots more aggressively
                    _logger.LogWarning("Blocked request from suspicious bot: {BotName} from IP: {IpAddress}", 
                        botName, ipAddress);
                    await BanIpAsync(ipAddress, $"Suspicious bot activity: {botName}", userAgent, null, config);
                    context.Response.StatusCode = 403; // Forbidden
                    await context.Response.WriteAsync("Access denied.");
                    return;
                }
            }
            else
            {
                // Check rate limits for regular users
                if (!CheckRateLimit(ipAddress, config.MaxRequestsPerMinute, 1))
                {
                    _logger.LogWarning("Rate limit exceeded (1-minute window) for IP: {IpAddress}", ipAddress);
                    await BanIpAsync(ipAddress, "Exceeded per-minute rate limit", userAgent, null, config);
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Too many requests. Please slow down.");
                    return;
                }
                
                if (!CheckRateLimit(ipAddress, config.MaxRequestsPer10Minutes, 10))
                {
                    _logger.LogWarning("Rate limit exceeded (10-minute window) for IP: {IpAddress}", ipAddress);
                    await BanIpAsync(ipAddress, "Exceeded 10-minute rate limit", userAgent, null, config);
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Too many requests. Your IP has been temporarily blocked.");
                    return;
                }
            }
            
            // Request is allowed, continue to next middleware
            await _next(context);
        }
        
        private bool CheckRateLimit(string ipAddress, int maxRequests, int windowMinutes)
        {
            var tracker = _requestTracking.GetOrAdd(ipAddress, _ => new RequestTracker());
            
            lock (tracker)
            {
                var now = DateTime.UtcNow;
                var windowStart = now.AddMinutes(-windowMinutes);
                
                // Remove old requests outside the window
                tracker.Requests.RemoveAll(r => r < windowStart);
                
                // Check if limit exceeded
                if (tracker.Requests.Count >= maxRequests)
                {
                    return false;
                }
                
                // Add current request
                tracker.Requests.Add(now);
                return true;
            }
        }
        
        private async Task BanIpAsync(string ipAddress, string reason, string? userAgent, string? attemptedEmail, RateLimitConfiguration config)
        {
            var tracker = _requestTracking.GetOrAdd(ipAddress, _ => new RequestTracker());
            
            lock (tracker)
            {
                tracker.BanCount++;
            }
            
            DateTime banExpiry;
            bool isPermanent = tracker.BanCount >= config.PermanentBanThreshold;
            
            if (isPermanent)
            {
                banExpiry = DateTime.MaxValue; // Permanent ban
            }
            else
            {
                banExpiry = DateTime.UtcNow.AddMinutes(config.BanDurationMinutes);
            }
            
            // Use BanManagementService to ensure cache and database are both updated
            using var scope = _scopeFactory.CreateScope();
            var banService = scope.ServiceProvider.GetRequiredService<BanManagementService>();
            
            await banService.BanIpAsync(
                ipAddress,
                reason,
                banExpiry,
                tracker.BanCount,
                userAgent ?? string.Empty,
                attemptedEmail ?? string.Empty);
        }
        
        private bool IsLegitimateBot(string botName)
        {
            // Allow well-known legitimate search engine bots
            var legitimateBots = new[] 
            { 
                "Google Bot", "Bing Bot", "Yahoo Bot", "DuckDuckGo Bot",
                "Baidu Spider", "YandexBot", "Applebot", "Baiduspider"
            };
            
            return legitimateBots.Any(bot => botName.Contains(bot, StringComparison.OrdinalIgnoreCase));
        }
        
        private class RequestTracker
        {
            public List<DateTime> Requests { get; set; } = new List<DateTime>();
            public int BanCount { get; set; } = 0;
        }
    }
}
