using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Utilities.Helpers;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Web.Middleware;

namespace digioz.Portal.Web.Filters
{
    /// <summary>
    /// Action filter attribute for password reset and sensitive authentication endpoints.
    /// Tracks attempts per email and IP to prevent enumeration attacks.
    /// Settings are read from Config table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class PasswordResetRateLimitAttribute : ActionFilterAttribute
    {
        private static readonly ConcurrentDictionary<string, List<DateTime>> _ipAttempts = new();
        private static readonly ConcurrentDictionary<string, List<DateTime>> _emailAttempts = new();
        
        // Cleanup timer to prevent memory leaks
        private static readonly System.Threading.Timer _cleanupTimer = new(CleanupOldAttempts, null, 
            TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));
        
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var ipAddress = IpAddressHelper.GetUserIPAddress(context.HttpContext);
            var logger = context.HttpContext.RequestServices
                .GetService(typeof(ILogger<PasswordResetRateLimitAttribute>)) as ILogger<PasswordResetRateLimitAttribute>;
            
            // Get configuration from services
            var configService = context.HttpContext.RequestServices.GetRequiredService<IConfigService>();
            var pluginService = context.HttpContext.RequestServices.GetRequiredService<IPluginService>();
            var configLogger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RateLimitConfiguration>>();
            
            var config = new RateLimitConfiguration(configService, pluginService, configLogger);
            
            // Check if rate limiting is enabled
            if (!config.IsEnabled)
            {
                base.OnActionExecuting(context);
                return;
            }
            
            // Check IP-based rate limit
            if (!CheckRateLimit(_ipAttempts, ipAddress, config.PasswordResetMaxAttemptsPerIpPerHour))
            {
                logger?.LogWarning("Password reset rate limit exceeded for IP: {IpAddress}", ipAddress);
                context.Result = new StatusCodeResult(429); // Too Many Requests
                return;
            }
            
            // Extract email from model if available (for POST requests)
            if (context.ActionArguments.TryGetValue("Input", out var inputModel))
            {
                var emailProperty = inputModel?.GetType().GetProperty("Email");
                if (emailProperty != null)
                {
                    var email = emailProperty.GetValue(inputModel)?.ToString();
                    if (!string.IsNullOrEmpty(email))
                    {
                        email = email.ToLowerInvariant();
                        
                        if (!CheckRateLimit(_emailAttempts, email, config.PasswordResetMaxAttemptsPerEmailPerHour))
                        {
                            logger?.LogWarning("Password reset rate limit exceeded for email: {Email} from IP: {IpAddress}", 
                                email, ipAddress);
                            // Still mark as rate limit exceeded but don't block the request
                            // This prevents email enumeration while still logging the attempt
                            context.HttpContext.Items["RateLimitExceeded"] = true;
                            context.HttpContext.Items["RateLimitEmail"] = email;
                        }
                    }
                }
            }
            
            base.OnActionExecuting(context);
        }
        
        private bool CheckRateLimit(ConcurrentDictionary<string, List<DateTime>> tracker, 
            string key, int maxAttempts)
        {
            var attempts = tracker.GetOrAdd(key, _ => new List<DateTime>());
            
            lock (attempts)
            {
                var now = DateTime.UtcNow;
                var oneHourAgo = now.AddHours(-1);
                
                // Remove attempts older than 1 hour
                attempts.RemoveAll(t => t < oneHourAgo);
                
                // Check if limit exceeded
                if (attempts.Count >= maxAttempts)
                {
                    return false;
                }
                
                // Add current attempt
                attempts.Add(now);
                return true;
            }
        }
        
        private static void CleanupOldAttempts(object? state)
        {
            var cutoff = DateTime.UtcNow.AddHours(-2);
            
            // Clean IP attempts
            foreach (var kvp in _ipAttempts.ToList())
            {
                var attempts = kvp.Value;
                lock (attempts)
                {
                    attempts.RemoveAll(t => t < cutoff);
                    if (attempts.Count == 0)
                    {
                        _ipAttempts.TryRemove(kvp.Key, out _);
                    }
                }
            }
            
            // Clean email attempts
            foreach (var kvp in _emailAttempts.ToList())
            {
                var attempts = kvp.Value;
                lock (attempts)
                {
                    attempts.RemoveAll(t => t < cutoff);
                    if (attempts.Count == 0)
                    {
                        _emailAttempts.TryRemove(kvp.Key, out _);
                    }
                }
            }
        }
    }
}
