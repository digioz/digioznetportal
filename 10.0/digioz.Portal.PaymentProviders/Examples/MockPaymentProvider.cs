namespace digioz.Portal.PaymentProviders.Examples
{
    using digioz.Portal.PaymentProviders.Models;

    /// <summary>
    /// Example of a custom payment provider implementation.
    /// This demonstrates how to extend the library with a new payment gateway.
    /// In this case, we're creating a mock/test provider for demonstration.
    /// </summary>
    public class MockPaymentProvider : BasePaymentProvider
    {
        public override string Name => "Mock";

        /// <summary>
        /// Validates that the provider is properly configured.
        /// For the mock provider, we just need an API key.
        /// </summary>
        public override bool ValidateConfiguration()
        {
            return !string.IsNullOrWhiteSpace(Config?.ApiKey);
        }

        /// <summary>
        /// Processes a payment transaction.
        /// In this mock implementation, we accept specific test card numbers:
        /// - 4111111111111111: Always approved
        /// - 4000000000000002: Always declined
        /// - Any other valid card: Returns error
        /// </summary>
        public override Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                ValidatePaymentRequest(request);

                if (!ValidateConfiguration())
                    return Task.FromResult(CreateErrorResponse("Mock provider not configured."));

                // Simulate different responses based on test card numbers
                if (request.CardNumber == "4111111111111111")
                {
                    // Approved
                    return Task.FromResult(new PaymentResponse
                    {
                        IsApproved = true,
                        TransactionId = Guid.NewGuid().ToString(),
                        AuthorizationCode = "123456",
                        Message = "Transaction approved",
                        ResponseCode = "1",
                        Amount = request.Amount,
                        RawResponse = new Dictionary<string, string>
                        {
                            { "status", "approved" }
                        }
                    });
                }
                else if (request.CardNumber == "4000000000000002")
                {
                    // Declined
                    return Task.FromResult(new PaymentResponse
                    {
                        IsApproved = false,
                        ErrorCode = "CARD_DECLINED",
                        ErrorMessage = "This card has been declined",
                        Message = "Card declined",
                        ResponseCode = "2",
                        Amount = request.Amount,
                        RawResponse = new Dictionary<string, string>
                        {
                            { "status", "declined" }
                        }
                    });
                }
                else
                {
                    // Invalid test card
                    return Task.FromResult(CreateErrorResponse(
                        "Invalid test card number for mock provider",
                        "INVALID_TEST_CARD"));
                }
            }
            catch (ArgumentException ex)
            {
                return Task.FromResult(CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return Task.FromResult(CreateErrorResponse(
                    $"An unexpected error occurred: {ex.Message}"));
            }
        }

        /// <summary>
        /// Refunds a previously approved transaction.
        /// Mock implementation always succeeds.
        /// </summary>
        public override Task<PaymentResponse> RefundAsync(string transactionId, decimal? amount = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(transactionId))
                    return Task.FromResult(CreateErrorResponse("Transaction ID is required"));

                if (!ValidateConfiguration())
                    return Task.FromResult(CreateErrorResponse("Mock provider not configured."));

                // Mock refund always succeeds
                return Task.FromResult(new PaymentResponse
                {
                    IsApproved = true,
                    TransactionId = Guid.NewGuid().ToString(),
                    Message = "Refund processed successfully",
                    ResponseCode = "1",
                    Amount = amount ?? 0,
                    RawResponse = new Dictionary<string, string>
                    {
                        { "status", "refunded" },
                        { "original_transaction_id", transactionId }
                    }
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(CreateErrorResponse($"Refund failed: {ex.Message}"));
            }
        }
    }

    /// <summary>
    /// Example: How to register the custom provider in your application.
    /// Add this to your Program.cs or Startup.cs:
    /// 
    /// services.AddPaymentProviders(builder =>
    /// {
    ///     builder.AddProvider&lt;MockPaymentProvider&gt;("Mock")
    ///            .ConfigureProvider("Mock", config =>
    ///            {
    ///                config.ApiKey = "mock-test-key";
    ///                config.IsTestMode = true;
    ///            });
    /// });
    /// </summary>
    public static class MockProviderExample { }
}
