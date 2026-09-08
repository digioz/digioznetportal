namespace digioz.Portal.PaymentProviders.Examples
{
    using digioz.Portal.PaymentProviders.Abstractions;
    using digioz.Portal.PaymentProviders.Models;

    /// <summary>
    /// Example service demonstrating payment provider usage.
    /// This shows how to integrate the payment providers library into your application.
    /// </summary>
    public class PaymentProcessingService
    {
        private readonly IPaymentProviderFactory _paymentProviderFactory;

        /// <summary>
        /// Constructor with dependency injection of the payment provider factory.
        /// </summary>
        public PaymentProcessingService(IPaymentProviderFactory paymentProviderFactory)
        {
            _paymentProviderFactory = paymentProviderFactory;
        }

        /// <summary>
        /// Example: Process a payment using the specified provider.
        /// </summary>
        public async Task<PaymentResponse> ProcessPaymentAsync(
            string providerName,
            decimal amount,
            string cardNumber,
            string expMonth,
            string expYear,
            string cvv,
            string cardholderName,
            string email,
            string billingAddress,
            string billingCity,
            string billingState,
            string billingZip,
            string billingCountry)
        {
            // Validate provider is available
            if (!_paymentProviderFactory.IsProviderAvailable(providerName))
            {
                throw new ArgumentException(
                    $"Payment provider '{providerName}' is not available. " +
                    $"Available providers: {string.Join(", ", _paymentProviderFactory.GetAvailableProviders())}",
                    nameof(providerName));
            }

            // Create provider instance
            var provider = _paymentProviderFactory.CreateProvider(providerName);

            // Build payment request
            var request = new PaymentRequest
            {
                TransactionId = Guid.NewGuid().ToString(),
                Amount = amount, // Amount in dollars (e.g., 19.90 for $19.90)
                CurrencyCode = "USD",
                CardNumber = cardNumber,
                ExpirationMonth = expMonth,
                ExpirationYear = expYear,
                CardCode = cvv,
                CardholderName = cardholderName,
                CustomerEmail = email,
                BillingAddress = billingAddress,
                BillingCity = billingCity,
                BillingState = billingState,
                BillingZip = billingZip,
                BillingCountry = billingCountry,
                Description = "Payment for order",
                InvoiceNumber = GenerateInvoiceNumber()
            };

            // Process payment
            var response = await provider.ProcessPaymentAsync(request);

            return response;
        }

        /// <summary>
        /// Example: Refund a transaction.
        /// </summary>
        public async Task<PaymentResponse> RefundPaymentAsync(
            string providerName,
            string transactionId,
            decimal? refundAmount = null)
        {
            if (!_paymentProviderFactory.IsProviderAvailable(providerName))
            {
                throw new ArgumentException(
                    $"Payment provider '{providerName}' is not available.",
                    nameof(providerName));
            }

            var provider = _paymentProviderFactory.CreateProvider(providerName);

            // Amount is already in dollars, pass it directly
            var response = await provider.RefundAsync(transactionId, refundAmount);

            return response;
        }

        /// <summary>
        /// Example: Get available payment providers.
        /// </summary>
        public IEnumerable<string> GetAvailableProviders()
        {
            return _paymentProviderFactory.GetAvailableProviders();
        }

        /// <summary>
        /// Generates a unique invoice number using a timestamp and GUID for uniqueness.
        /// Format: INV-{timestamp}-{guid-segment}
        /// This avoids collision risks by using the full GUID information.
        /// </summary>
        private static string GenerateInvoiceNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var guidSegment = Guid.NewGuid().ToString("N").Substring(0, 8);
            return $"INV-{timestamp}-{guidSegment}";
        }
    }
}
