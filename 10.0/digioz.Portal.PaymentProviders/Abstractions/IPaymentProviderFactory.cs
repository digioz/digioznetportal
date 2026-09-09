namespace digioz.Portal.PaymentProviders.Abstractions
{
    using digioz.Portal.PaymentProviders.Models;

    /// <summary>
    /// Factory interface for creating payment provider instances.
    /// </summary>
    public interface IPaymentProviderFactory
    {
        /// <summary>
        /// Creates a payment provider instance by name.
        /// </summary>
        /// <param name="providerName">The name of the provider (e.g., "AuthorizeNet", "PayPal").</param>
        /// <param name="scopedServiceProvider">Optional scoped service provider from the current request. If provided, scoped services will be resolved from this provider.</param>
        /// <returns>An instance of the requested payment provider.</returns>
        /// <exception cref="ArgumentException">Thrown if the provider name is not registered.</exception>
        IPaymentProvider CreateProvider(string providerName, IServiceProvider? scopedServiceProvider = null);

        /// <summary>
        /// Gets all registered provider names.
        /// </summary>
        /// <returns>A list of registered provider names.</returns>
        IEnumerable<string> GetAvailableProviders();

        /// <summary>
        /// Checks if a provider is registered.
        /// </summary>
        /// <param name="providerName">The name of the provider.</param>
        /// <returns>True if the provider is registered, false otherwise.</returns>
        bool IsProviderAvailable(string providerName);
    }
}
