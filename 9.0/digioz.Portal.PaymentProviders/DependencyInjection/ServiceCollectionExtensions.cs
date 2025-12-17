namespace digioz.Portal.PaymentProviders.DependencyInjection
{
    using digioz.Portal.PaymentProviders.Abstractions;
    using digioz.Portal.PaymentProviders.Models;
    using digioz.Portal.PaymentProviders.Providers;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension methods for registering payment providers with the dependency injection container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds payment provider support to the service collection with default providers (AuthorizeNet and PayPal).
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Optional action to configure the payment providers.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddPaymentProviders(
            this IServiceCollection services,
            Action<PaymentProviderBuilder>? configure = null)
        {
            var builder = new PaymentProviderBuilder(services);

            // Register default providers
            builder.AddProvider<AuthorizeNetProvider>("AuthorizeNet")
                   .AddProvider<PayPalProvider>("PayPal");

            // Allow custom configuration
            configure?.Invoke(builder);

            // Register factory
            services.AddSingleton(sp => builder.Build(sp));
            services.AddSingleton<IPaymentProviderFactory>(sp => sp.GetRequiredService<PaymentProviderFactory>());

            return services;
        }

        /// <summary>
        /// Adds payment provider support to the service collection from a dictionary configuration.
        /// Dictionary keys should be provider names, values should be comma-separated settings.
        /// Format: "ApiKey=value,ApiSecret=value,MerchantId=value,IsTestMode=true"
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="providerConfigurations">Dictionary mapping provider names to configuration strings.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddPaymentProvidersFromDictionary(
            this IServiceCollection services,
            Dictionary<string, string> providerConfigurations)
        {
            if (providerConfigurations == null)
                throw new ArgumentNullException(nameof(providerConfigurations));

            return services.AddPaymentProviders(builder =>
            {
                foreach (var providerConfig in providerConfigurations)
                {
                    builder.ConfigureProviderFromDictionary(providerConfig.Key, providerConfig.Value);
                }
            });
        }

        /// <summary>
        /// Adds a single payment provider without default providers.
        /// </summary>
        /// <typeparam name="T">The payment provider type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="providerName">The name to register the provider under.</param>
        /// <param name="config">The provider configuration.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddPaymentProvider<T>(
            this IServiceCollection services,
            string providerName,
            PaymentProviderConfig config)
            where T : class, IPaymentProvider
        {
            services.AddScoped<T>();
            services.AddSingleton(sp =>
            {
                var factory = new PaymentProviderFactory(sp);
                factory.RegisterProvider(providerName, typeof(T));
                factory.RegisterConfiguration(providerName, config);
                return factory;
            });
            services.AddSingleton<IPaymentProviderFactory>(sp => sp.GetRequiredService<PaymentProviderFactory>());

            return services;
        }
    }

    /// <summary>
    /// Builder for configuring payment providers.
    /// </summary>
    public class PaymentProviderBuilder
    {
        private readonly IServiceCollection _services;
        private readonly PaymentProviderFactory _factory;
        private readonly Dictionary<string, PaymentProviderConfig> _configurations;

        public PaymentProviderBuilder(IServiceCollection services)
        {
            _services = services;
            _factory = new PaymentProviderFactory();
            _configurations = new Dictionary<string, PaymentProviderConfig>();
        }

        /// <summary>
        /// Adds a payment provider to the builder.
        /// </summary>
        /// <typeparam name="T">The payment provider type.</typeparam>
        /// <param name="name">The name to register the provider under.</param>
        /// <returns>This builder for chaining.</returns>
        public PaymentProviderBuilder AddProvider<T>(string name)
            where T : class, IPaymentProvider
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Provider name cannot be empty.", nameof(name));

            _factory.RegisterProvider(name, typeof(T));
            _services.AddScoped<T>();

            return this;
        }

        /// <summary>
        /// Configures a provider with settings.
        /// </summary>
        /// <param name="providerName">The provider name.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>This builder for chaining.</returns>
        public PaymentProviderBuilder ConfigureProvider(string providerName, PaymentProviderConfig config)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider name cannot be empty.", nameof(providerName));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _configurations[providerName] = config;
            _factory.RegisterConfiguration(providerName, config);

            return this;
        }

        /// <summary>
        /// Configures a provider with an action.
        /// </summary>
        /// <param name="providerName">The provider name.</param>
        /// <param name="configure">The configuration action.</param>
        /// <returns>This builder for chaining.</returns>
        public PaymentProviderBuilder ConfigureProvider(string providerName, Action<PaymentProviderConfig> configure)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider name cannot be empty.", nameof(providerName));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var config = new PaymentProviderConfig();
            configure(config);
            _configurations[providerName] = config;
            _factory.RegisterConfiguration(providerName, config);

            return this;
        }

        /// <summary>
        /// Configures a provider from a dictionary or comma-separated configuration string.
        /// Format: "ApiKey=value,ApiSecret=value,MerchantId=value,IsTestMode=true"
        /// </summary>
        /// <param name="providerName">The provider name.</param>
        /// <param name="configurationString">The configuration string with key=value pairs separated by commas.</param>
        /// <returns>This builder for chaining.</returns>
        public PaymentProviderBuilder ConfigureProviderFromDictionary(string providerName, string configurationString)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider name cannot be empty.", nameof(providerName));

            if (string.IsNullOrWhiteSpace(configurationString))
                throw new ArgumentException("Configuration string cannot be empty.", nameof(configurationString));

            var config = new PaymentProviderConfig();
            var settings = configurationString.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var setting in settings)
            {
                var keyValue = setting.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();

                    switch (key.ToLowerInvariant())
                    {
                        case "apikey":
                            config.ApiKey = value;
                            break;
                        case "apisecret":
                            config.ApiSecret = value;
                            break;
                        case "merchantid":
                            config.MerchantId = value;
                            break;
                        case "istestmode":
                            if (bool.TryParse(value, out var testMode))
                                config.IsTestMode = testMode;
                            break;
                        default:
                            // Store unknown settings in Options dictionary
                            config.Options[key] = value;
                            break;
                    }
                }
            }

            _configurations[providerName] = config;
            _factory.RegisterConfiguration(providerName, config);

            return this;
        }

        /// <summary>
        /// Builds the payment provider factory.
        /// </summary>
        internal PaymentProviderFactory Build(IServiceProvider serviceProvider)
        {
            return new PaymentProviderFactory(serviceProvider)
            {
                _registeredProviders = _factory._registeredProviders,
                _configurations = _configurations
            };
        }
    }
}
