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
                Amount = amount * 100, // Convert to cents (smallest currency unit)
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
                InvoiceNumber = Guid.NewGuid().ToString().Substring(0, 12)
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

            // Convert amount to cents if provided
            var amount = refundAmount.HasValue ? refundAmount.Value * 100 : (decimal?)null;

            var response = await provider.RefundAsync(transactionId, amount);

            return response;
        }

        /// <summary>
        /// Example: Get available payment providers.
        /// </summary>
        public IEnumerable<string> GetAvailableProviders()
        {
            return _paymentProviderFactory.GetAvailableProviders();
        }
    }
}
