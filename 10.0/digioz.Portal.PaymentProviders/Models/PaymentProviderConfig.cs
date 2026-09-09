namespace digioz.Portal.PaymentProviders.Models
{
    /// <summary>
    /// Represents the configuration for a payment provider.
    /// </summary>
    public class PaymentProviderConfig
    {
        /// <summary>
        /// API key or merchant key for the payment provider.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// API secret or transaction key for the payment provider.
        /// </summary>
        public string? ApiSecret { get; set; }

        /// <summary>
        /// Merchant ID for the payment provider.
        /// </summary>
        public string? MerchantId { get; set; }

        /// <summary>
        /// Indicates whether to use sandbox/test mode.
        /// </summary>
        public bool IsTestMode { get; set; } = true;

        /// <summary>
        /// Additional configuration options (key-value pairs).
        /// </summary>
        public Dictionary<string, string> Options { get; set; } = new();
    }
}
