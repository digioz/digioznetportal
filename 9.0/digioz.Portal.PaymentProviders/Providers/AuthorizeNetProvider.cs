namespace digioz.Portal.PaymentProviders.Providers
{
    using digioz.Portal.PaymentProviders.Models;
    using System.Net.Http.Json;

    /// <summary>
    /// Authorize.net payment provider implementation.
    /// Supports AIM (Advanced Integration Method) API.
    /// </summary>
    public class AuthorizeNetProvider : BasePaymentProvider
    {
        private readonly HttpClient _httpClient;
        private const string SandboxUrl = "https://test.authorize.net/gateway/transact.dll";
        private const string ProductionUrl = "https://secure.authorize.net/gateway/transact.dll";

        public override string Name => "AuthorizeNet";

        /// <summary>
        /// Constructor that accepts HttpClient via dependency injection.
        /// </summary>
        /// <param name="httpClient">HttpClient instance managed by the DI container.</param>
        public AuthorizeNetProvider(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public override bool ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(Config?.ApiKey))
                return false;

            if (string.IsNullOrWhiteSpace(Config?.ApiSecret))
                return false;

            return true;
        }

        public override async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                ValidatePaymentRequest(request);

                if (!ValidateConfiguration())
                    return CreateErrorResponse("Authorize.net provider not properly configured.");

                var payload = BuildChargePayload(request);
                var response = await SendRequestAsync(payload);

                return ParseResponse(response);
            }
            catch (ArgumentException ex)
            {
                return CreateErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse($"An error occurred processing the payment: {ex.Message}");
            }
        }

        public override async Task<PaymentResponse> RefundAsync(string transactionId, decimal? amount = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(transactionId))
                    return CreateErrorResponse("Transaction ID is required for refund.");

                if (!ValidateConfiguration())
                    return CreateErrorResponse("Authorize.net provider not properly configured.");

                var payload = BuildRefundPayload(transactionId, amount);
                var response = await SendRequestAsync(payload);

                return ParseResponse(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse($"An error occurred processing the refund: {ex.Message}");
            }
        }

        private Dictionary<string, string> BuildChargePayload(PaymentRequest request)
        {
            var payload = new Dictionary<string, string>
            {
                { "x_login", Config?.ApiKey ?? "" },
                { "x_tran_key", Config?.ApiSecret ?? "" },
                { "x_version", "3.1" },
                { "x_delim_data", "true" },
                { "x_delim_char", "," },
                { "x_relay_response", "FALSE" },
                { "x_type", "AUTH_CAPTURE" },
                { "x_method", "CC" },
                { "x_card_num", request.CardNumber ?? "" },
                { "x_exp_date", $"{request.ExpirationMonth}/{request.ExpirationYear}" },
                { "x_card_code", request.CardCode ?? "" },
                { "x_amount", request.Amount.ToString("F2") },
                { "x_currency_code", request.CurrencyCode },
                { "x_description", request.Description ?? "" },
                { "x_invoice_num", request.InvoiceNumber ?? "" },
                { "x_cust_id", request.TransactionId ?? "" },
                { "x_email", request.CustomerEmail ?? "" },
                { "x_phone", request.CustomerPhone ?? "" },
                { "x_first_name", request.CardholderName ?? "" },
                { "x_address", request.BillingAddress ?? "" },
                { "x_city", request.BillingCity ?? "" },
                { "x_state", request.BillingState ?? "" },
                { "x_zip", request.BillingZip ?? "" },
                { "x_country", request.BillingCountry ?? "" },
                { "x_ship_to_first_name", request.CardholderName ?? "" },
                { "x_ship_to_address", request.ShippingAddress ?? "" },
                { "x_ship_to_city", request.ShippingCity ?? "" },
                { "x_ship_to_state", request.ShippingState ?? "" },
                { "x_ship_to_zip", request.ShippingZip ?? "" },
                { "x_ship_to_country", request.ShippingCountry ?? "" },
            };

            return payload;
        }

        private Dictionary<string, string> BuildRefundPayload(string transactionId, decimal? amount)
        {
            var payload = new Dictionary<string, string>
            {
                { "x_login", Config?.ApiKey ?? "" },
                { "x_tran_key", Config?.ApiSecret ?? "" },
                { "x_version", "3.1" },
                { "x_delim_data", "true" },
                { "x_delim_char", "," },
                { "x_relay_response", "FALSE" },
                { "x_type", "CREDIT" },
                { "x_method", "CC" },
                { "x_trans_id", transactionId },
            };

            if (amount.HasValue)
                payload["x_amount"] = amount.Value.ToString("F2");

            return payload;
        }

        private async Task<string> SendRequestAsync(Dictionary<string, string> payload)
        {
            var url = Config?.IsTestMode == true ? SandboxUrl : ProductionUrl;

            var content = new FormUrlEncodedContent(payload);
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"HTTP error: {response.StatusCode}");

            return await response.Content.ReadAsStringAsync();
        }

        private PaymentResponse ParseResponse(string responseContent)
        {
            var parts = responseContent.Split(',');

            if (parts.Length < 4)
                return CreateErrorResponse("Invalid response from Authorize.net", "INVALID_RESPONSE");

            var responseCode = parts[0].Trim();
            var responseReasonCode = parts[2].Trim();
            var responseReasonText = parts[3].Trim();
            var authCode = parts.Length > 4 ? parts[4].Trim() : "";
            var transId = parts.Length > 6 ? parts[6].Trim() : "";

            var isApproved = responseCode == "1"; // 1 = Approved, 2 = Declined, 3 = Error, 4 = Held for review

            var response = new PaymentResponse
            {
                IsApproved = isApproved,
                ResponseCode = responseCode,
                AuthorizationCode = authCode,
                TransactionId = transId,
                Message = responseReasonText,
                ErrorMessage = !isApproved ? responseReasonText : null,
                ErrorCode = !isApproved ? responseReasonCode : null,
                RawResponse = new Dictionary<string, string>
                {
                    { "ResponseCode", responseCode },
                    { "ResponseReasonCode", responseReasonCode },
                    { "ResponseReasonText", responseReasonText }
                }
            };

            return response;
        }
    }
}
