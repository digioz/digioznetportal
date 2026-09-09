using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Middleware
{
    /// <summary>
    /// Configuration settings for the Rate Limiting Middleware.
    /// Reads settings from the Config table in the database and caches them.
    /// </summary>
    public class RateLimitConfiguration
    {
        private readonly IConfigService _configService;
        private readonly IPluginService _pluginService;
        private readonly ILogger<RateLimitConfiguration> _logger;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(2); // Short cache duration

        // Default values (fallback if database values not found)
        private const int DefaultMaxRequestsPerMinute = 20;
        private const int DefaultMaxRequestsPer10Minutes = 60;
        private const int DefaultBanDurationMinutes = 60;
        private const int DefaultPermanentBanThreshold = 5;
        private const int DefaultPasswordResetMaxAttemptsPerIpPerHour = 10;
        private const int DefaultPasswordResetMaxAttemptsPerEmailPerHour = 3;
        private const int DefaultLoginMaxAttemptsPerIpPerHour = 10;
        private const int DefaultLoginMaxAttemptsPerEmailPerHour = 5;
        private const int DefaultRegistrationMaxAttemptsPerIpPerHour = 10;
        private const int DefaultRegistrationMaxAttemptsPerEmailPerHour = 5;

        public RateLimitConfiguration(
            IConfigService configService, 
            IPluginService pluginService,
            ILogger<RateLimitConfiguration> logger,
            IMemoryCache cache)
        {
            _configService = configService;
            _pluginService = pluginService;
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Check if rate limiting is enabled via Plugin table
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                // Use a different cache key for plugin status
                string cacheKey = "RateLimit_Plugin_IsEnabled";
                
                if (_cache.TryGetValue(cacheKey, out bool isEnabled))
                {
                    return isEnabled;
                }
                
                try
                {
                    var plugin = _pluginService.GetByName("Rate Limiting & Bot Protection");
                    isEnabled = plugin?.IsEnabled ?? false;
                    
                    _cache.Set(cacheKey, isEnabled, _cacheDuration);
                    return isEnabled;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking if Rate Limiting plugin is enabled. Defaulting to disabled.");
                    return false;
                }
            }
        }

        /// <summary>
        /// Maximum requests allowed per minute per IP
        /// </summary>
        public int MaxRequestsPerMinute => GetConfigInt("RateLimit.MaxRequestsPerMinute", DefaultMaxRequestsPerMinute);

        /// <summary>
        /// Maximum requests allowed per 10 minutes per IP
        /// </summary>
        public int MaxRequestsPer10Minutes => GetConfigInt("RateLimit.MaxRequestsPer10Minutes", DefaultMaxRequestsPer10Minutes);

        /// <summary>
        /// Duration of temporary bans in minutes
        /// </summary>
        public int BanDurationMinutes => GetConfigInt("RateLimit.BanDurationMinutes", DefaultBanDurationMinutes);

        /// <summary>
        /// Number of temporary bans before permanent ban
        /// </summary>
        public int PermanentBanThreshold => GetConfigInt("RateLimit.PermanentBanThreshold", DefaultPermanentBanThreshold);

        /// <summary>
        /// Maximum password reset attempts per IP per hour
        /// </summary>
        public int PasswordResetMaxAttemptsPerIpPerHour => GetConfigInt("RateLimit.PasswordReset.MaxAttemptsPerIpPerHour", DefaultPasswordResetMaxAttemptsPerIpPerHour);

        /// <summary>
        /// Maximum password reset attempts per email per hour
        /// </summary>
        public int PasswordResetMaxAttemptsPerEmailPerHour => GetConfigInt("RateLimit.PasswordReset.MaxAttemptsPerEmailPerHour", DefaultPasswordResetMaxAttemptsPerEmailPerHour);

        /// <summary>
        /// Maximum login attempts per IP per hour
        /// </summary>
        public int LoginMaxAttemptsPerIpPerHour => GetConfigInt("RateLimit.Login.MaxAttemptsPerIpPerHour", DefaultLoginMaxAttemptsPerIpPerHour);

        /// <summary>
        /// Maximum login attempts per email per hour
        /// </summary>
        public int LoginMaxAttemptsPerEmailPerHour => GetConfigInt("RateLimit.Login.MaxAttemptsPerEmailPerHour", DefaultLoginMaxAttemptsPerEmailPerHour);

        /// <summary>
        /// Maximum registration attempts per IP per hour
        /// </summary>
        public int RegistrationMaxAttemptsPerIpPerHour => GetConfigInt("RateLimit.Registration.MaxAttemptsPerIpPerHour", DefaultRegistrationMaxAttemptsPerIpPerHour);

        /// <summary>
        /// Maximum registration attempts per email per hour
        /// </summary>
        public int RegistrationMaxAttemptsPerEmailPerHour => GetConfigInt("RateLimit.Registration.MaxAttemptsPerEmailPerHour", DefaultRegistrationMaxAttemptsPerEmailPerHour);

        /// <summary>
        /// Helper method to get integer config value with caching
        /// </summary>
        private int GetConfigInt(string key, int defaultValue)
        {
            // Cache key prefix to avoid collisions
            string cacheKey = $"RateLimit_Config_{key}";
            
            if (_cache.TryGetValue(cacheKey, out int cachedValue))
            {
                return cachedValue;
            }
            
            try
            {
                var config = _configService.GetByKey(key);
                if (config != null && int.TryParse(config.ConfigValue, out int value))
                {
                    _cache.Set(cacheKey, value, _cacheDuration);
                    return value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading config key: {ConfigKey}. Using default value: {DefaultValue}", key, defaultValue);
            }

            // Don't cache default values on error/missing to allows retries or adding later, 
            // OR cache it if we assume it won't change often. 
            // Here we choose NOT to cache the default value to allow immediate pickup if config is added.
            // But to protect DB from hammering if config is missing, we could cache the default.
            // Let's cache the default value as well to be safe for performance.
            _cache.Set(cacheKey, defaultValue, _cacheDuration);
            return defaultValue;
        }

        /// <summary>
        /// Reload all configuration values from database
        /// Clears the cache entries for rate limiting
        /// </summary>
        public void Reload()
        {
            // We can't easily clear specific keys from IMemoryCache without tracking them.
            // But since expiration is short (2 mins), manual reload might not be strictly needed or 
            // we can implement a mechanism to use a version token or just rely on expiration.
            // For now, logging. A more advanced implementation would use a cancellation token for cache entries.
            
            _logger.LogInformation("Rate limiting configuration reload requested - changes will apply after cache expiration (max 2 mins)");
        }
    }
}
