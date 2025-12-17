namespace digioz.Portal.PaymentProviders.Abstractions
{
    using digioz.Portal.PaymentProviders.Models;

    /// <summary>
    /// Interface for payment providers. Implement this to add support for new payment gateways.
    /// </summary>
    public interface IPaymentProvider
    {
        /// <summary>
        /// Gets the name/identifier of the payment provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Processes a payment transaction.
        /// </summary>
        /// <param name="request">The payment request containing transaction details.</param>
        /// <returns>A task that returns the payment response.</returns>
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);

        /// <summary>
        /// Refunds a previously approved transaction.
        /// </summary>
        /// <param name="transactionId">The original transaction ID to refund.</param>
        /// <param name="amount">The amount to refund. If null, refunds the full amount.</param>
        /// <returns>A task that returns the refund response.</returns>
        Task<PaymentResponse> RefundAsync(string transactionId, decimal? amount = null);

        /// <summary>
        /// Validates the payment provider configuration.
        /// </summary>
        /// <returns>True if the configuration is valid, false otherwise.</returns>
        bool ValidateConfiguration();
    }
}
