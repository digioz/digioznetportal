using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Utilities.Helpers;
using digioz.Portal.Utilities;
using digioz.Portal.Web.Data;
using digioz.Portal.Web.Services;

namespace digioz.Portal.Web.Middleware
{
    /// <summary>
    /// Middleware that implements IP-based rate limiting with automatic ban enforcement.
    /// Tracks requests per IP address in the BannedIpTracking table and blocks excessive requests.
    /// Cleanup of expired data is handled by RateLimitCleanupService.
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

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
            try
            {
                // Get client IP address
                var ipAddress = IpAddressHelper.GetUserIPAddress(context);
                var path = context.Request.Path.Value ?? string.Empty;

                // IMPORTANT: Skip rate limiting for the RateLimited page to prevent redirect loops
                if (path.Equals("/RateLimited", StringComparison.OrdinalIgnoreCase))
                {
                    await _next(context);
                    return;
                }

                // Check if rate limiting is enabled via Plugin configuration
                using var scope = _scopeFactory.CreateScope();
                var configService = scope.ServiceProvider.GetRequiredService<Dal.Services.Interfaces.IConfigService>();
                var pluginService = scope.ServiceProvider.GetRequiredService<Dal.Services.Interfaces.IPluginService>();
                var configLogger = scope.ServiceProvider.GetRequiredService<ILogger<RateLimitConfiguration>>();
                var cache = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
                
                var config = new RateLimitConfiguration(configService, pluginService, configLogger, cache);
                
                if (!config.IsEnabled)
                {
                    // Rate limiting is disabled, skip all checks
                    await _next(context);
                    return;
                }
                
                _logger.LogInformation("Rate limiting is ENABLED - checking limits");
                
                // Check if IP is banned FIRST (before any rate limit checks)
                // This is important to block already-banned IPs immediately
                using var banScope = _scopeFactory.CreateScope();
                var banService = banScope.ServiceProvider.GetRequiredService<BanManagementService>();
                var banCheckResult = await banService.IsBannedAsync(ipAddress);
                
                if (banCheckResult.IsBanned && banCheckResult.BanInfo != null)
                {
                    var retryAfter = banCheckResult.BanInfo.IsPermanent 
                        ? "permanently" 
                        : $"{(banCheckResult.BanInfo.BanExpiry - DateTime.UtcNow).TotalMinutes:F0} minutes";
                    
                    _logger.LogWarning("Blocked request from banned IP: {IpAddress}. Ban expires: {Expiry}. Reason: {Reason}", 
                        ipAddress, 
                        banCheckResult.BanInfo.IsPermanent ? "Permanent" : banCheckResult.BanInfo.BanExpiry.ToString(), 
                        banCheckResult.BanInfo.Reason);
                    
                    // Check if response has already started
                    if (!context.Response.HasStarted)
                    {
                        // Redirect to user-friendly rate limited page
                        context.Response.Redirect($"/RateLimited?reason={Uri.EscapeDataString(banCheckResult.BanInfo.Reason)}&retryAfter={Uri.EscapeDataString(retryAfter)}");
                    }
                    else
                    {
                        _logger.LogWarning("Cannot redirect - response already started");
                    }
                    return;
                }
                
                // IMPORTANT: Skip rate limiting for static files to prevent false positives
                // Only track dynamic requests (pages, API calls, form submissions)
                if (ShouldSkipRateLimiting(context))
                {
                    await _next(context);
                    return;
                }
                
                // Check for bot and rate limit
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                var isBot = BotHelper.IsBot(userAgent);
                
                // Get rate limit service from scope
                var rateLimitService = scope.ServiceProvider.GetRequiredService<RateLimitService>();
                
                // Skip tracking for pages that have specialized filters
                // This allows specialized tracking with email information
                bool isSpecialPage = path.Contains("/forgotpassword", StringComparison.OrdinalIgnoreCase) ||
                                    path.Contains("/resetpassword", StringComparison.OrdinalIgnoreCase) ||
                                    path.Contains("/login", StringComparison.OrdinalIgnoreCase) ||
                                    path.Contains("/register", StringComparison.OrdinalIgnoreCase);
                
                if (!isSpecialPage)
                {
                    // Track this request in BannedIpTracking table
                    await rateLimitService.TrackRequestAsync(ipAddress, path, "General", null, userAgent);
                }
                
                if (isBot)
                {
                    var botName = BotHelper.ExtractBotName(userAgent);
                    
                    if (!await rateLimitService.CheckRateLimitAsync(ipAddress, config.MaxRequestsPerMinute / 2, 1))
                    {
                        _logger.LogWarning("Rate limit exceeded for legitimate bot: {BotName} from IP: {IpAddress}", 
                            botName, ipAddress);
                        if (!context.Response.HasStarted)
                        {
                            context.Response.Redirect("/RateLimited?reason=Rate%20limit%20exceeded%20for%20crawlers&retryAfter=1%20minute");
                            return;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Blocked request from suspicious bot: {BotName} from IP: {IpAddress}", 
                            botName, ipAddress);
                        await BanIpAsync(ipAddress, $"Suspicious bot activity: {botName}", userAgent, null, config);
                        if (!context.Response.HasStarted)
                        {
                            context.Response.Redirect("/RateLimited?reason=Suspicious%20bot%20activity%20detected&retryAfter=1%20hour");
                        }
                        return;
                    }
                }
                else
                {
                    // Check rate limits for regular users
                    if (!await rateLimitService.CheckRateLimitAsync(ipAddress, config.MaxRequestsPerMinute, 1))
                    {
                        _logger.LogWarning("Rate limit exceeded (1-minute window) for IP: {IpAddress}. Path: {Path}", 
                            ipAddress, context.Request.Path);
                        
                        await BanIpAsync(ipAddress, "Exceeded per-minute rate limit", userAgent, null, config);
                        
                        if (!context.Response.HasStarted)
                        {
                            context.Response.Redirect("/RateLimited?reason=Too%20many%20requests%20per%20minute&retryAfter=1%20minute");
                        }
                        return;
                    }
                    
                    if (!await rateLimitService.CheckRateLimitAsync(ipAddress, config.MaxRequestsPer10Minutes, 10))
                    {
                        _logger.LogWarning("Rate limit exceeded (10-minute window) for IP: {IpAddress}. Path: {Path}", 
                            ipAddress, context.Request.Path);
                        
                        await BanIpAsync(ipAddress, "Exceeded 10-minute rate limit", userAgent, null, config);
                        
                        if (!context.Response.HasStarted)
                        {
                            context.Response.Redirect("/RateLimited?reason=Too%20many%20requests%20in%2010%20minutes&retryAfter=10%20minutes");
                        }
                        return;
                    }
                }
                
                // Request is allowed, continue to next middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "===== RATE LIMITING MIDDLEWARE EXCEPTION ===== Path: {Path}", 
                    context.Request.Path);
                // Re-throw to let ASP.NET Core handle it
                throw;
            }
        }
        
        /// <summary>
        /// Determines if rate limiting should be skipped for this request.
        /// Static files (CSS, JS, images, fonts) are not rate-limited to prevent
        /// false positives where normal page loads trigger bans.
        /// </summary>
        private bool ShouldSkipRateLimiting(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            
            // Skip static file extensions
            var staticExtensions = new[]
            {
                ".css", ".js", ".map",                          // Stylesheets and scripts
                ".jpg", ".jpeg", ".png", ".gif", ".svg", ".ico", ".webp", // Images
                ".woff", ".woff2", ".ttf", ".eot", ".otf",      // Fonts
                ".mp4", ".webm", ".ogg",                        // Videos
                ".mp3", ".wav",                                 // Audio
                ".pdf", ".zip", ".txt",                         // Documents
                ".xml", ".json"                                 // Data files (when served as static)
            };
            
            if (staticExtensions.Any(ext => path.EndsWith(ext)))
            {
                return true;
            }
            
            // Skip common static file paths
            if (path.StartsWith("/css/") || 
                path.StartsWith("/js/") || 
                path.StartsWith("/images/") || 
                path.StartsWith("/img/") ||
                path.StartsWith("/fonts/") ||
                path.StartsWith("/lib/") ||
                path.StartsWith("/assets/") ||
                path.StartsWith("/static/") ||
                path.StartsWith("/favicon.ico"))
            {
                return true;
            }
            
            // Skip ASP.NET Core SignalR and hot reload (development)
            if (path.StartsWith("/_framework/") || 
                path.StartsWith("/_vs/") ||
                path.Contains("/signalr/"))
            {
                return true;
            }
            
            return false;
        }
        
        private async Task BanIpAsync(string ipAddress, string reason, string? userAgent, string? attemptedEmail, RateLimitConfiguration config)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var banService = scope.ServiceProvider.GetRequiredService<BanManagementService>();
                
                // Get existing ban count from database
                var existingBans = await dbContext.BannedIp
                    .Where(b => b.IpAddress == ipAddress)
                    .OrderByDescending(b => b.CreatedDate)
                    .FirstOrDefaultAsync();
                
                int banCount = existingBans?.BanCount + 1 ?? 1;
                
                bool isPermanent = banCount >= config.PermanentBanThreshold;
                DateTime banExpiry = isPermanent 
                    ? DateTime.MaxValue 
                    : DateTime.UtcNow.AddMinutes(config.BanDurationMinutes);
                
                await banService.BanIpAsync(
                    ipAddress,
                    reason,
                    banExpiry,
                    banCount,
                    userAgent ?? string.Empty,
                    attemptedEmail ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ban IP: {IP}", ipAddress);
                throw;
            }
        }
    }
}
