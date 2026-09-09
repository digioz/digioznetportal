namespace digioz.Portal.PaymentProviders.Models
{
    /// <summary>
    /// Represents a payment request to be processed by a payment provider.
    /// </summary>
    public class PaymentRequest
    {
        /// <summary>
        /// Unique identifier for the payment transaction.
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// The amount to charge in dollars (e.g., 19.90 for $19.90).
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Three-letter ISO currency code (e.g., USD, EUR).
        /// </summary>
        public string CurrencyCode { get; set; } = "USD";

        /// <summary>
        /// Credit card number (PAN).
        /// </summary>
        public string? CardNumber { get; set; }

        /// <summary>
        /// Card expiration month (MM format).
        /// </summary>
        public string? ExpirationMonth { get; set; }

        /// <summary>
        /// Card expiration year (YYYY format).
        /// </summary>
        public string? ExpirationYear { get; set; }

        /// <summary>
        /// Card Verification Value/Security code (CVV/CVC).
        /// </summary>
        public string? CardCode { get; set; }

        /// <summary>
        /// Cardholder name.
        /// </summary>
        public string? CardholderName { get; set; }

        /// <summary>
        /// Transaction description/order information.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Customer email address.
        /// </summary>
        public string? CustomerEmail { get; set; }

        /// <summary>
        /// Customer phone number.
        /// </summary>
        public string? CustomerPhone { get; set; }

        /// <summary>
        /// Billing address - street.
        /// </summary>
        public string? BillingAddress { get; set; }

        /// <summary>
        /// Billing address - city.
        /// </summary>
        public string? BillingCity { get; set; }

        /// <summary>
        /// Billing address - state/province.
        /// </summary>
        public string? BillingState { get; set; }

        /// <summary>
        /// Billing address - postal code.
        /// </summary>
        public string? BillingZip { get; set; }

        /// <summary>
        /// Billing address - country.
        /// </summary>
        public string? BillingCountry { get; set; }

        /// <summary>
        /// Shipping address - street.
        /// </summary>
        public string? ShippingAddress { get; set; }

        /// <summary>
        /// Shipping address - city.
        /// </summary>
        public string? ShippingCity { get; set; }

        /// <summary>
        /// Shipping address - state/province.
        /// </summary>
        public string? ShippingState { get; set; }

        /// <summary>
        /// Shipping address - postal code.
        /// </summary>
        public string? ShippingZip { get; set; }

        /// <summary>
        /// Shipping address - country.
        /// </summary>
        public string? ShippingCountry { get; set; }

        /// <summary>
        /// Invoice number for reference.
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// Additional customer data (key-value pairs).
        /// </summary>
        public Dictionary<string, string> CustomFields { get; set; } = new();
    }
}
