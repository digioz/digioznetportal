namespace digioz.Portal.PaymentProviders.Providers
{
    using digioz.Portal.PaymentProviders.Models;
    using System.Web;

    /// <summary>
    /// PayPal payment provider implementation.
    /// Supports PayPal Direct Payment API (legacy) and can be extended for REST API.
    /// </summary>
    public class PayPalProvider : BasePaymentProvider
    {
        private readonly HttpClient _httpClient;
        private const string SandboxUrl = "https://api.sandbox.paypal.com/nvp";
        private const string ProductionUrl = "https://api.paypal.com/nvp";
        private const string ApiVersion = "204.0";

        public override string Name => "PayPal";

        public PayPalProvider(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public override bool ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(Config?.ApiKey))
                return false;

            if (string.IsNullOrWhiteSpace(Config?.ApiSecret))
                return false;

            if (string.IsNullOrWhiteSpace(Config?.MerchantId))
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
            var payload = new Dictionary<string, string>
            {
                { "METHOD", "DoDirectPayment" },
                { "VERSION", ApiVersion },
                { "USER", Config?.ApiKey ?? "" },
                { "PWD", Config?.ApiSecret ?? "" },
                { "SIGNATURE", Config?.MerchantId ?? "" },
                { "PAYMENTACTION", "Sale" },
                { "IPADDRESS", "127.0.0.1" }, // Should be actual client IP in production
                { "CREDITCARDTYPE", "Visa" }, // Could be enhanced to detect from card number
                { "ACCT", request.CardNumber ?? "" },
                { "EXPDATE", $"{request.ExpirationMonth}{request.ExpirationYear}" },
                { "CVV2", request.CardCode ?? "" },
                { "FIRSTNAME", ExtractFirstName(request.CardholderName) },
                { "LASTNAME", ExtractLastName(request.CardholderName) },
                { "EMAIL", request.CustomerEmail ?? "" },
                { "PHONENUM", request.CustomerPhone ?? "" },
                { "STREET", request.BillingAddress ?? "" },
                { "CITY", request.BillingCity ?? "" },
                { "STATE", request.BillingState ?? "" },
                { "ZIP", request.BillingZip ?? "" },
                { "COUNTRYCODE", request.BillingCountry ?? "US" },
                { "SHIPTONAME", request.CardholderName ?? "" },
                { "SHIPTOSTREET", request.ShippingAddress ?? "" },
                { "SHIPTOCITY", request.ShippingCity ?? "" },
                { "SHIPTOSTATE", request.ShippingState ?? "" },
                { "SHIPTOZIP", request.ShippingZip ?? "" },
                { "SHIPTOCOUNTRYCODE", request.ShippingCountry ?? "US" },
                { "AMT", request.Amount.ToString("F2") },
                { "CURRENCYCODE", request.CurrencyCode },
                { "DESC", request.Description ?? "" },
                { "INVNUM", request.InvoiceNumber ?? "" },
                { "CUSTOM", request.TransactionId ?? "" },
            };

            return payload;
        }

        private Dictionary<string, string> BuildRefundPayload(string transactionId, decimal? amount)
        {
            var payload = new Dictionary<string, string>
            {
                { "METHOD", "RefundTransaction" },
                { "VERSION", ApiVersion },
                { "USER", Config?.ApiKey ?? "" },
                { "PWD", Config?.ApiSecret ?? "" },
                { "SIGNATURE", Config?.MerchantId ?? "" },
                { "TRANSACTIONID", transactionId },
            };

            if (amount.HasValue)
                payload["REFUNDTYPE"] = "Partial";

            if (amount.HasValue)
                payload["AMT"] = amount.Value.ToString("F2");

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
            var responseDict = ParseNVP(responseContent);

            if (!responseDict.TryGetValue("ACK", out var ack))
                return CreateErrorResponse("Invalid response from PayPal", "INVALID_RESPONSE");

            var isApproved = ack == "Success" || ack == "SuccessWithWarning";

            var response = new PaymentResponse
            {
                IsApproved = isApproved,
                ResponseCode = ack,
                AuthorizationCode = responseDict.GetValueOrDefault("AUTHORIZATIONID") ?? 
                                   responseDict.GetValueOrDefault("TRANSACTIONID") ?? "",
                TransactionId = responseDict.GetValueOrDefault("TRANSACTIONID") ?? "",
                Message = responseDict.GetValueOrDefault("L_LONGMESSAGE0") ?? ack,
                ErrorMessage = !isApproved ? (responseDict.GetValueOrDefault("L_LONGMESSAGE0") ?? ack) : null,
                ErrorCode = !isApproved ? responseDict.GetValueOrDefault("L_ERRORCODE0") : null,
                RawResponse = responseDict
            };

            return response;
        }

        private Dictionary<string, string> ParseNVP(string responseContent)
        {
            var result = new Dictionary<string, string>();
            var pairs = responseContent.Split('&');

            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    var key = HttpUtility.UrlDecode(parts[0]);
                    var value = HttpUtility.UrlDecode(parts[1]);
                    if (key != null && value != null)
                    {
                        result[key] = value;
                    }
                }
            }

            return result;
        }

        private string ExtractFirstName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "";

            var parts = fullName.Split(' ');
            return parts[0];
        }

        private string ExtractLastName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "";

            var parts = fullName.Split(' ');
            if (parts.Length > 1)
                return string.Join(" ", parts.Skip(1));

            return "";
        }
    }
}
