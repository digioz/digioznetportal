using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using digioz.Portal.Utilities.Helpers;
using digioz.Portal.Web.Services;
using digioz.Portal.Web.Middleware;

namespace digioz.Portal.Web.Filters
{
    /// <summary>
    /// Async page filter for login rate limiting.
    /// Tracks login attempts per email and IP to prevent brute force attacks.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class LoginRateLimitAttribute : Attribute, IAsyncPageFilter
    {
        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var ipAddress = IpAddressHelper.GetUserIPAddress(context.HttpContext);
            var logger = context.HttpContext.RequestServices
                .GetService(typeof(ILogger<LoginRateLimitAttribute>)) as ILogger<LoginRateLimitAttribute>;
            
            // Get services
            var rateLimitService = context.HttpContext.RequestServices.GetRequiredService<RateLimitService>();
            var configService = context.HttpContext.RequestServices.GetRequiredService<Dal.Services.Interfaces.IConfigService>();
            var pluginService = context.HttpContext.RequestServices.GetRequiredService<Dal.Services.Interfaces.IPluginService>();
            var configLogger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RateLimitConfiguration>>();
            var cache = context.HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
            
            var config = new RateLimitConfiguration(configService, pluginService, configLogger, cache);
            
            // Check if rate limiting is enabled
            if (!config.IsEnabled)
            {
                await next();
                return;
            }
            
            // Extract email from handler arguments if available (for POST requests)
            string? email = null;
            if (context.HandlerArguments != null && context.HandlerArguments.Count > 0)
            {
                foreach (var arg in context.HandlerArguments.Values)
                {
                    if (arg != null)
                    {
                        var emailProperty = arg.GetType().GetProperty("Email");
                        if (emailProperty != null)
                        {
                            email = emailProperty.GetValue(arg)?.ToString();
                            if (!string.IsNullOrEmpty(email))
                            {
                                email = email.ToLowerInvariant();
                                break;
                            }
                        }
                    }
                }
            }
            
            var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();

            // Check IP-based rate limit (per hour)
            if (!await rateLimitService.CheckLoginLimitPerIpAsync(
                ipAddress, 
                config.LoginMaxAttemptsPerIpPerHour, 
                1)) // 1 hour window
            {
                logger?.LogWarning("Login limit exceeded (IP) - IP: {IP}, Max: {Max}/hour", 
                    ipAddress, config.LoginMaxAttemptsPerIpPerHour);
                
                // Ban the IP for exceeding login limit
                var banService = context.HttpContext.RequestServices.GetRequiredService<Services.BanManagementService>();
                
                // Get existing ban count to enable escalation
                int currentBanCount = await banService.GetBanCountAsync(ipAddress);
                int newBanCount = currentBanCount + 1;
                
                DateTime banExpiry;
                if (newBanCount >= config.PermanentBanThreshold)
                {
                    banExpiry = DateTime.MaxValue; // Permanent ban
                }
                else
                {
                    banExpiry = DateTime.UtcNow.AddHours(1); // 1 hour ban for login abuse
                }
                
                await banService.BanIpAsync(ipAddress, "Exceeded login limit", banExpiry, newBanCount, userAgent, email ?? "");
                    
                context.Result = new Microsoft.AspNetCore.Mvc.RedirectResult(
                    $"/RateLimited?reason={Uri.EscapeDataString($"Too many login attempts (max {config.LoginMaxAttemptsPerIpPerHour}/hour)")}&retryAfter=1%20hour");
                return;
            }
            
            // Check email-based rate limit if email was provided
            if (!string.IsNullOrEmpty(email))
            {
                if (!await rateLimitService.CheckLoginLimitPerEmailAsync(
                    email, 
                    config.LoginMaxAttemptsPerEmailPerHour, 
                    1)) // 1 hour window
                {
                    logger?.LogWarning("Login limit exceeded (email) - Email: {Email}, Max: {Max}/hour", 
                        email, config.LoginMaxAttemptsPerEmailPerHour);
                    
                    // Ban the IP for exceeding email-based login limit
                    var banService = context.HttpContext.RequestServices.GetRequiredService<Services.BanManagementService>();
                    
                    // Get existing ban count to enable escalation
                    int currentBanCount = await banService.GetBanCountAsync(ipAddress);
                    int newBanCount = currentBanCount + 1;
                    
                    DateTime banExpiry;
                    if (newBanCount >= config.PermanentBanThreshold)
                    {
                        banExpiry = DateTime.MaxValue; // Permanent ban
                    }
                    else
                    {
                        banExpiry = DateTime.UtcNow.AddHours(1); // 1 hour ban
                    }
                    
                    await banService.BanIpAsync(ipAddress, $"Exceeded login limit for email: {email}", banExpiry, newBanCount, userAgent, email);
                    
                    context.Result = new Microsoft.AspNetCore.Mvc.RedirectResult(
                        $"/RateLimited?reason={Uri.EscapeDataString($"Too many login attempts for this email (max {config.LoginMaxAttemptsPerEmailPerHour}/hour)")}&retryAfter=1%20hour");
                    return;
                }
            }

            // Track this login request
            await rateLimitService.TrackRequestAsync(ipAddress, "/Identity/Account/Login", "Login", email, userAgent);
            
            // Continue execution
            await next();
        }
    }
}
