namespace digioz.Portal.PaymentProviders.Providers
{
    using digioz.Portal.PaymentProviders.Models;

    /// <summary>
    /// PayPal payment provider implementation.
    /// Uses classic NVP API shape but configuration now expects REST-style ClientId/ClientSecret.
    /// </summary>
    public class PayPalProvider : BasePaymentProvider
    {
        private readonly HttpClient _httpClient;
        private const string SandboxUrl = "https://api-3t.sandbox.paypal.com/nvp";
        private const string ProductionUrl = "https://api-3t.paypal.com/nvp";
        private const string ApiVersion = "204.0";

        public override string Name => "PayPal";

        public PayPalProvider(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public override bool ValidateConfiguration()
        {
            // For REST-style config we use ApiKey = ClientId, ApiSecret = ClientSecret.
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
                    return CreateErrorResponse("PayPal provider not properly configured.");

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
                    return CreateErrorResponse("PayPal provider not properly configured.");

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
            // NOTE: This still uses classic NVP DoDirectPayment for card processing.
            // Config.ApiKey and Config.ApiSecret are treated as API username/password which
            // must be provisioned for the NVP API even if the naming is REST-style.
            var payload = new Dictionary<string, string>
            {
                { "METHOD", "DoDirectPayment" },
                { "VERSION", ApiVersion },
                { "USER", Config?.ApiKey ?? string.Empty },
                { "PWD", Config?.ApiSecret ?? string.Empty },
                { "SIGNATURE", Config?.Options.GetValueOrDefault("Signature") ?? string.Empty },
                { "PAYMENTACTION", "Sale" },
                { "IPADDRESS", "127.0.0.1" },
                { "CREDITCARDTYPE", "Visa" },
                { "ACCT", request.CardNumber ?? string.Empty },
                { "EXPDATE", $"{request.ExpirationMonth}{request.ExpirationYear}" },
                { "CVV2", request.CardCode ?? string.Empty },
                { "FIRSTNAME", ExtractFirstName(request.CardholderName) },
                { "LASTNAME", ExtractLastName(request.CardholderName) },
                { "EMAIL", request.CustomerEmail ?? string.Empty },
                { "PHONENUM", request.CustomerPhone ?? string.Empty },
                { "STREET", request.BillingAddress ?? string.Empty },
                { "CITY", request.BillingCity ?? string.Empty },
                { "STATE", request.BillingState ?? string.Empty },
                { "ZIP", request.BillingZip ?? string.Empty },
                { "COUNTRYCODE", request.BillingCountry ?? "US" },
                { "SHIPTONAME", request.CardholderName ?? string.Empty },
                { "SHIPTOSTREET", request.ShippingAddress ?? string.Empty },
                { "SHIPTOCITY", request.ShippingCity ?? string.Empty },
                { "SHIPTOSTATE", request.ShippingState ?? string.Empty },
                { "SHIPTOZIP", request.ShippingZip ?? string.Empty },
                { "SHIPTOCOUNTRYCODE", request.ShippingCountry ?? "US" },
                { "AMT", request.Amount.ToString("F2") },
                { "CURRENCYCODE", request.CurrencyCode },
                { "DESC", request.Description ?? string.Empty },
                { "INVNUM", request.InvoiceNumber ?? string.Empty },
                { "CUSTOM", request.TransactionId ?? string.Empty }
            };

            return payload;
        }

        private Dictionary<string, string> BuildRefundPayload(string transactionId, decimal? amount)
        {
            var payload = new Dictionary<string, string>
            {
                { "METHOD", "RefundTransaction" },
                { "VERSION", ApiVersion },
                { "USER", Config?.ApiKey ?? string.Empty },
                { "PWD", Config?.ApiSecret ?? string.Empty },
                { "SIGNATURE", Config?.Options.GetValueOrDefault("Signature") ?? string.Empty },
                { "TRANSACTIONID", transactionId }
            };

            if (amount.HasValue)
            {
                payload["REFUNDTYPE"] = "Partial";
                payload["AMT"] = amount.Value.ToString("F2");
            }

            return payload;
        }

        private async Task<string> SendRequestAsync(Dictionary<string, string> payload)
        {
            var url = Config?.IsTestMode == true ? SandboxUrl : ProductionUrl;

            using var content = new FormUrlEncodedContent(payload);
            using var response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"HTTP error: {response.StatusCode}");

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private PaymentResponse ParseResponse(string responseContent)
        {
            var responseDict = ParseNvp(responseContent);

            if (!responseDict.TryGetValue("ACK", out var ack))
                return CreateErrorResponse("Invalid response from PayPal", "INVALID_RESPONSE");

            var isApproved = ack == "Success" || ack == "SuccessWithWarning";

            return new PaymentResponse
            {
                IsApproved = isApproved,
                ResponseCode = ack,
                AuthorizationCode = responseDict.GetValueOrDefault("AUTHORIZATIONID") ??
                                   responseDict.GetValueOrDefault("TRANSACTIONID") ?? string.Empty,
                TransactionId = responseDict.GetValueOrDefault("TRANSACTIONID") ?? string.Empty,
                Message = responseDict.GetValueOrDefault("L_LONGMESSAGE0") ?? ack,
                ErrorMessage = !isApproved ? (responseDict.GetValueOrDefault("L_LONGMESSAGE0") ?? ack) : null,
                ErrorCode = !isApproved ? responseDict.GetValueOrDefault("L_ERRORCODE0") : null,
                RawResponse = responseDict
            };
        }

        private static Dictionary<string, string> ParseNvp(string responseContent)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var pairs = responseContent.Split('&', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length != 2)
                    continue;

                var key = System.Web.HttpUtility.UrlDecode(parts[0]);
                var value = System.Web.HttpUtility.UrlDecode(parts[1]);

                if (!string.IsNullOrEmpty(key))
                    result[key] = value ?? string.Empty;
            }

            return result;
        }

        private static string ExtractFirstName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;

            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : string.Empty;
        }

        private static string ExtractLastName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;

            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;
        }
    }
}
