namespace digioz.Portal.PaymentProviders.Examples
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using digioz.Portal.PaymentProviders.DependencyInjection;

    /// <summary>
    /// Example configuration for setting up payment providers in a web application.
    /// This demonstrates how to integrate with ASP.NET Core dependency injection.
    /// </summary>
    public static class PaymentProviderStartup
    {
        /// <summary>
        /// Configures payment providers with settings from appsettings.json.
        /// 
        /// Example appsettings.json structure:
        /// {
        ///   "PaymentProviders": {
        ///     "AuthorizeNet": {
        ///       "ApiKey": "YOUR_LOGIN_ID",
        ///       "ApiSecret": "YOUR_TRANSACTION_KEY",
        ///       "IsTestMode": true
        ///     },
        ///     "PayPal": {
        ///       "ApiKey": "API_USERNAME",
        ///       "ApiSecret": "API_PASSWORD",
        ///       "MerchantId": "API_SIGNATURE",
        ///       "IsTestMode": true
        ///     }
        ///   }
        /// }
        /// </summary>
        public static IServiceCollection ConfigurePaymentProviders(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddPaymentProviders(builder =>
            {
                var section = configuration.GetSection("PaymentProviders");

                // Configure Authorize.net
                if (section.GetSection("AuthorizeNet").Exists())
                {
                    builder.ConfigureProvider("AuthorizeNet", config =>
                    {
                        config.ApiKey = section["AuthorizeNet:ApiKey"];
                        config.ApiSecret = section["AuthorizeNet:ApiSecret"];
                        var testModeStr = section["AuthorizeNet:IsTestMode"] ?? "true";
                        config.IsTestMode = bool.Parse(testModeStr);
                    });
                }

                // Configure PayPal
                if (section.GetSection("PayPal").Exists())
                {
                    builder.ConfigureProvider("PayPal", config =>
                    {
                        config.ApiKey = section["PayPal:ApiKey"];
                        config.ApiSecret = section["PayPal:ApiSecret"];
                        config.MerchantId = section["PayPal:MerchantId"];
                        var testModeStr = section["PayPal:IsTestMode"] ?? "true";
                        config.IsTestMode = bool.Parse(testModeStr);
                    });
                }
            });

            return services;
        }

        /// <summary>
        /// Alternative: Configure with environment variables.
        /// Looks for variables like:
        /// - PAYMENT_AUTHORIZENET_APIKEY
        /// - PAYMENT_AUTHORIZENET_APISECRET
        /// - PAYMENT_PAYPAL_APIKEY
        /// - etc.
        /// </summary>
        public static IServiceCollection ConfigurePaymentProvidersFromEnvironment(
            this IServiceCollection services)
        {
            services.AddPaymentProviders(builder =>
            {
                var prefix = "PAYMENT_";

                // Configure from environment variables
                var authNetKey = Environment.GetEnvironmentVariable($"{prefix}AUTHORIZENET_APIKEY");
                var authNetSecret = Environment.GetEnvironmentVariable($"{prefix}AUTHORIZENET_APISECRET");
                var authNetTestMode = Environment.GetEnvironmentVariable($"{prefix}AUTHORIZENET_TESTMODE");

                if (!string.IsNullOrEmpty(authNetKey) && !string.IsNullOrEmpty(authNetSecret))
                {
                    builder.ConfigureProvider("AuthorizeNet", config =>
                    {
                        config.ApiKey = authNetKey;
                        config.ApiSecret = authNetSecret;
                        config.IsTestMode = string.IsNullOrEmpty(authNetTestMode) ? true : bool.Parse(authNetTestMode);
                    });
                }

                var paypalKey = Environment.GetEnvironmentVariable($"{prefix}PAYPAL_APIKEY");
                var paypalSecret = Environment.GetEnvironmentVariable($"{prefix}PAYPAL_APISECRET");
                var paypalMerchantId = Environment.GetEnvironmentVariable($"{prefix}PAYPAL_MERCHANTID");
                var paypalTestMode = Environment.GetEnvironmentVariable($"{prefix}PAYPAL_TESTMODE");

                if (!string.IsNullOrEmpty(paypalKey) && !string.IsNullOrEmpty(paypalSecret))
                {
                    builder.ConfigureProvider("PayPal", config =>
                    {
                        config.ApiKey = paypalKey;
                        config.ApiSecret = paypalSecret;
                        config.MerchantId = paypalMerchantId ?? "";
                        config.IsTestMode = string.IsNullOrEmpty(paypalTestMode) ? true : bool.Parse(paypalTestMode);
                    });
                }
            });

            return services;
        }

        /// <summary>
        /// Configure with a dictionary of key-value pairs.
        /// Dictionary keys should be provider names (e.g., "AuthorizeNet", "PayPal").
        /// Dictionary values should be comma-separated settings in the format: "ApiKey=value,ApiSecret=value,MerchantId=value,IsTestMode=true"
        /// 
        /// Example usage in Program.cs:
        /// var providerConfigs = new Dictionary&lt;string, string&gt;
        /// {
        ///     { "AuthorizeNet", "ApiKey=LOGIN_ID,ApiSecret=TRANSACTION_KEY,IsTestMode=true" },
        ///     { "PayPal", "ApiKey=USERNAME,ApiSecret=PASSWORD,MerchantId=SIGNATURE,IsTestMode=true" }
        /// };
        /// services.AddPaymentProvidersFromDictionary(providerConfigs);
        /// </summary>
        public static IServiceCollection ConfigurePaymentProvidersFromDictionary(
            this IServiceCollection services,
            Dictionary<string, string> providerConfigurations)
        {
            return services.AddPaymentProvidersFromDictionary(providerConfigurations);
        }
    }
}
