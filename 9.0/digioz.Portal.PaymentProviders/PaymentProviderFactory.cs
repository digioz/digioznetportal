namespace digioz.Portal.PaymentProviders
{
    using digioz.Portal.PaymentProviders.Abstractions;
    using digioz.Portal.PaymentProviders.Models;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Factory for creating and managing payment provider instances.
    /// Uses dependency injection to provide registered providers.
    /// </summary>
    public class PaymentProviderFactory : IPaymentProviderFactory
    {
        private readonly IServiceProvider? _serviceProvider;
        internal Dictionary<string, Type> _registeredProviders;
        internal Dictionary<string, PaymentProviderConfig> _configurations;

        public PaymentProviderFactory(IServiceProvider? serviceProvider = null)
        {
            _serviceProvider = serviceProvider;
            _registeredProviders = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            _configurations = new Dictionary<string, PaymentProviderConfig>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registers a payment provider type.
        /// </summary>
        /// <param name="name">The name to register the provider under.</param>
        /// <param name="providerType">The type implementing IPaymentProvider.</param>
        /// <exception cref="ArgumentException">Thrown if type doesn't implement IPaymentProvider.</exception>
        public void RegisterProvider(string name, Type providerType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Provider name cannot be empty.", nameof(name));

            if (!typeof(IPaymentProvider).IsAssignableFrom(providerType))
                throw new ArgumentException($"Type {providerType.Name} must implement IPaymentProvider.", nameof(providerType));

            _registeredProviders[name] = providerType;
        }

        /// <summary>
        /// Registers a payment provider configuration.
        /// </summary>
        /// <param name="name">The provider name.</param>
        /// <param name="config">The payment provider configuration.</param>
        public void RegisterConfiguration(string name, PaymentProviderConfig config)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Provider name cannot be empty.", nameof(name));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _configurations[name] = config;
        }

        public IPaymentProvider CreateProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider name cannot be empty.", nameof(providerName));

            if (!_registeredProviders.TryGetValue(providerName, out var providerType))
                throw new ArgumentException($"Payment provider '{providerName}' is not registered.", nameof(providerName));

            BasePaymentProvider? provider = null;

            try
            {
                if (_serviceProvider != null)
                {
                    provider = _serviceProvider.GetService(providerType) as BasePaymentProvider;
                }

                if (provider == null)
                {
                    provider = Activator.CreateInstance(providerType) as BasePaymentProvider;
                }

                if (provider == null)
                    throw new InvalidOperationException($"Failed to create instance of {providerType.Name}.");

                if (_configurations.TryGetValue(providerName, out var config))
                {
                    provider.Initialize(config);
                }

                return provider;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create payment provider '{providerName}': {ex.Message}", ex);
            }
        }

        public IEnumerable<string> GetAvailableProviders()
        {
            return new ReadOnlyCollection<string>(_registeredProviders.Keys.ToList());
        }

        public bool IsProviderAvailable(string providerName)
        {
            return !string.IsNullOrWhiteSpace(providerName) && 
                   _registeredProviders.ContainsKey(providerName);
        }
    }
}
