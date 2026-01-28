using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace digioz.Portal.Web.Middleware
{
    /// <summary>
    /// Configuration settings for the Rate Limiting Middleware.
    /// Reads settings from the Config table in the database.
    /// </summary>
    public class RateLimitConfiguration
    {
        private readonly IConfigService _configService;
        private readonly IPluginService _pluginService;
        private readonly ILogger<RateLimitConfiguration> _logger;

        // Default values (fallback if database values not found)
        private const int DefaultMaxRequestsPerMinute = 20;
        private const int DefaultMaxRequestsPer10Minutes = 60;
        private const int DefaultBanDurationMinutes = 60;
        private const int DefaultPermanentBanThreshold = 5;
        private const int DefaultPasswordResetMaxAttemptsPerIpPerHour = 10;
        private const int DefaultPasswordResetMaxAttemptsPerEmailPerHour = 3;

        public RateLimitConfiguration(
            IConfigService configService, 
            IPluginService pluginService,
            ILogger<RateLimitConfiguration> logger)
        {
            _configService = configService;
            _pluginService = pluginService;
            _logger = logger;
        }

        /// <summary>
        /// Check if rate limiting is enabled via Plugin table
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                try
                {
                    var plugin = _pluginService.GetByName("Rate Limiting & Bot Protection");
                    return plugin?.IsEnabled ?? false;
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
        /// Helper method to get integer config value
        /// </summary>
        private int GetConfigInt(string key, int defaultValue)
        {
            try
            {
                var config = _configService.GetByKey(key);
                if (config != null && int.TryParse(config.ConfigValue, out int value))
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading config key: {ConfigKey}. Using default value: {DefaultValue}", key, defaultValue);
            }

            return defaultValue;
        }

        /// <summary>
        /// Reload all configuration values from database
        /// Call this after updating config values in admin panel
        /// </summary>
        public void Reload()
        {
            // Force property access to reload from database
            _ = MaxRequestsPerMinute;
            _ = MaxRequestsPer10Minutes;
            _ = BanDurationMinutes;
            _ = PermanentBanThreshold;
            _ = PasswordResetMaxAttemptsPerIpPerHour;
            _ = PasswordResetMaxAttemptsPerEmailPerHour;
            
            _logger.LogInformation("Rate limiting configuration reloaded from database");
        }
    }
}
