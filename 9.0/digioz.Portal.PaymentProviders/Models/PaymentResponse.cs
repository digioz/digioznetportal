namespace digioz.Portal.PaymentProviders.Models
{
    /// <summary>
    /// Represents the response from a payment provider after processing a transaction.
    /// </summary>
    public class PaymentResponse
    {
        /// <summary>
        /// Whether the transaction was approved.
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Authorization code from the payment provider.
        /// </summary>
        public string? AuthorizationCode { get; set; }

        /// <summary>
        /// Transaction ID from the payment provider.
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// Response message from the payment provider.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Response code from the payment provider (e.g., "1" for approved).
        /// </summary>
        public string? ResponseCode { get; set; }

        /// <summary>
        /// The amount that was charged.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Error details if the transaction failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Error code from the payment provider.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Raw response data from the payment provider (for debugging).
        /// </summary>
        public Dictionary<string, string> RawResponse { get; set; } = new();

        /// <summary>
        /// Additional data returned by the provider (key-value pairs).
        /// </summary>
        public Dictionary<string, string> CustomData { get; set; } = new();
    }
}
