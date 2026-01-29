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
    /// Async page filter for registration rate limiting.
    /// Tracks registration attempts per email and IP to prevent spam and abuse.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RegistrationRateLimitAttribute : Attribute, IAsyncPageFilter
    {
        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var ipAddress = IpAddressHelper.GetUserIPAddress(context.HttpContext);
            var logger = context.HttpContext.RequestServices
                .GetService(typeof(ILogger<RegistrationRateLimitAttribute>)) as ILogger<RegistrationRateLimitAttribute>;
            
            // Get services
            var rateLimitService = context.HttpContext.RequestServices.GetRequiredService<RateLimitService>();
            var configService = context.HttpContext.RequestServices.GetRequiredService<Dal.Services.Interfaces.IConfigService>();
            var pluginService = context.HttpContext.RequestServices.GetRequiredService<Dal.Services.Interfaces.IPluginService>();
            var configLogger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RateLimitConfiguration>>();
            
            var config = new RateLimitConfiguration(configService, pluginService, configLogger);
            
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
            
            // Track this registration request
            var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();
            await rateLimitService.TrackRequestAsync(ipAddress, "/Identity/Account/Register", "Registration", email, userAgent);
            
            // Check IP-based rate limit (per hour)
            if (!await rateLimitService.CheckRegistrationLimitPerIpAsync(
                ipAddress, 
                config.RegistrationMaxAttemptsPerIpPerHour, 
                1)) // 1 hour window
            {
                logger?.LogWarning("Registration limit exceeded (IP) - IP: {IP}, Max: {Max}/hour", 
                    ipAddress, config.RegistrationMaxAttemptsPerIpPerHour);
                
                // Ban the IP for exceeding registration limit
                var banService = context.HttpContext.RequestServices.GetRequiredService<Services.BanManagementService>();
                var banExpiry = DateTime.UtcNow.AddHours(1); // 1 hour ban for registration abuse
                await banService.BanIpAsync(ipAddress, "Exceeded registration limit", banExpiry, 1, userAgent, email ?? "");
                    
                context.Result = new Microsoft.AspNetCore.Mvc.RedirectResult(
                    $"/RateLimited?reason={Uri.EscapeDataString($"Too many registration attempts (max {config.RegistrationMaxAttemptsPerIpPerHour}/hour)")}&retryAfter=1%20hour");
                return;
            }
            
            // Check email-based rate limit if email was provided
            if (!string.IsNullOrEmpty(email))
            {
                if (!await rateLimitService.CheckRegistrationLimitPerEmailAsync(
                    email, 
                    config.RegistrationMaxAttemptsPerEmailPerHour, 
                    1)) // 1 hour window
                {
                    logger?.LogWarning("Registration limit exceeded (email) - Email: {Email}, Max: {Max}/hour", 
                        email, config.RegistrationMaxAttemptsPerEmailPerHour);
                    
                    // Ban the IP for exceeding email-based registration limit
                    var banService = context.HttpContext.RequestServices.GetRequiredService<Services.BanManagementService>();
                    var banExpiry = DateTime.UtcNow.AddHours(1); // 1 hour ban
                    await banService.BanIpAsync(ipAddress, $"Exceeded registration limit for email: {email}", banExpiry, 1, userAgent, email);
                    
                    context.Result = new Microsoft.AspNetCore.Mvc.RedirectResult(
                        $"/RateLimited?reason={Uri.EscapeDataString($"Too many registration attempts for this email (max {config.RegistrationMaxAttemptsPerEmailPerHour}/hour)")}&retryAfter=1%20hour");
                    return;
                }
            }
            
            // Continue execution
            await next();
        }
    }
}
