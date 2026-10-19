namespace digioz.Portal.PaymentProviders
{
    using digioz.Portal.PaymentProviders.Abstractions;
    using digioz.Portal.PaymentProviders.Models;

    /// <summary>
    /// Base class for payment provider implementations.
    /// Provides common functionality and validation.
    /// </summary>
    public abstract class BasePaymentProvider : IPaymentProvider
    {
        protected PaymentProviderConfig? Config { get; private set; }

        /// <summary>
        /// Gets the name of the payment provider.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Initializes the payment provider with configuration.
        /// </summary>
        /// <param name="config">The payment provider configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown if config is null.</exception>
        public void Initialize(PaymentProviderConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            Config = config;
        }

        /// <summary>
        /// Processes a payment transaction.
        /// </summary>
        public abstract Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);

        /// <summary>
        /// Refunds a previously approved transaction.
        /// </summary>
        public abstract Task<PaymentResponse> RefundAsync(string transactionId, decimal? amount = null);

        /// <summary>
        /// Validates the payment provider configuration.
        /// </summary>
        public abstract bool ValidateConfiguration();

        /// <summary>
        /// Helper method to validate payment request.
        /// </summary>
        protected void ValidatePaymentRequest(PaymentRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.CardNumber))
                throw new ArgumentException("Card number is required.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.ExpirationMonth) || 
                string.IsNullOrWhiteSpace(request.ExpirationYear))
                throw new ArgumentException("Card expiration date is required.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.CardCode))
                throw new ArgumentException("Card code (CVV) is required.", nameof(request));

            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(request));
        }

        /// <summary>
        /// Helper method to create a generic error response.
        /// </summary>
        protected PaymentResponse CreateErrorResponse(string errorMessage, string? errorCode = null)
        {
            return new PaymentResponse
            {
                IsApproved = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                Message = errorMessage
            };
        }
    }
}
