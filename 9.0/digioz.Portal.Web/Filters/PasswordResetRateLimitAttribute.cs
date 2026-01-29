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
    /// Async page filter for password reset rate limiting.
    /// Tracks attempts per email and IP to prevent enumeration attacks.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class PasswordResetRateLimitAttribute : Attribute, IAsyncPageFilter
    {
        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var ipAddress = IpAddressHelper.GetUserIPAddress(context.HttpContext);
            var logger = context.HttpContext.RequestServices
                .GetService(typeof(ILogger<PasswordResetRateLimitAttribute>)) as ILogger<PasswordResetRateLimitAttribute>;
            
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
            
            // Track this password reset request (whether email found or not)
            var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();
            await rateLimitService.TrackRequestAsync(ipAddress, "/Identity/Account/ForgotPassword", "ForgotPassword", email, userAgent);
            
            // Check IP-based rate limit (per hour)
            if (!await rateLimitService.CheckPasswordResetLimitPerIpAsync(
                ipAddress, 
                config.PasswordResetMaxAttemptsPerIpPerHour, 
                1)) // 1 hour window
            {
                logger?.LogWarning("Password reset limit exceeded (IP) - IP: {IP}, Max: {Max}/hour", 
                    ipAddress, config.PasswordResetMaxAttemptsPerIpPerHour);
                
                // Ban the IP for exceeding password reset limit
                var banService = context.HttpContext.RequestServices.GetRequiredService<Services.BanManagementService>();
                var banExpiry = DateTime.UtcNow.AddHours(1); // 1 hour ban for password reset abuse
                await banService.BanIpAsync(ipAddress, "Exceeded password reset limit", banExpiry, 1, userAgent ?? "", email ?? "");
                    
                context.Result = new Microsoft.AspNetCore.Mvc.RedirectResult(
                    $"/RateLimited?reason={Uri.EscapeDataString($"Too many password reset attempts (max {config.PasswordResetMaxAttemptsPerIpPerHour}/hour)")}&retryAfter=1%20hour");
                return;
            }
            
            // Check email-based rate limit if email was provided
            if (!string.IsNullOrEmpty(email))
            {
                if (!await rateLimitService.CheckPasswordResetLimitPerEmailAsync(
                    email, 
                    config.PasswordResetMaxAttemptsPerEmailPerHour, 
                    1)) // 1 hour window
                {
                    logger?.LogWarning("Password reset limit exceeded (email) - Email: {Email}, Max: {Max}/hour", 
                        email, config.PasswordResetMaxAttemptsPerEmailPerHour);
                    
                    // Don't block - prevents email enumeration
                    // Just mark in context for logging
                    context.HttpContext.Items["RateLimitExceeded"] = true;
                    context.HttpContext.Items["RateLimitEmail"] = email;
                }
            }
            
            // Continue execution
            await next();
        }
    }
}
