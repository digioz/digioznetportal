namespace digioz.Portal.PaymentProviders.Providers
{
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using digioz.Portal.PaymentProviders.Models;

    /// <summary>
    /// PayPal payment provider implementation using the REST API (Orders + capture).
    /// Config.ApiKey = ClientId, Config.ApiSecret = ClientSecret.
    /// </summary>
    public class PayPalProvider : BasePaymentProvider
    {
        private readonly HttpClient _httpClient;

        private const string SandboxBaseUrl = "https://api-m.sandbox.paypal.com";
        private const string ProductionBaseUrl = "https://api-m.paypal.com";

        public override string Name => "PayPal";

        public PayPalProvider(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public override bool ValidateConfiguration()
        {
            // REST config: ApiKey = ClientId, ApiSecret = ClientSecret.
            if (string.IsNullOrWhiteSpace(Config?.ApiKey))
                return false;

            if (string.IsNullOrWhiteSpace(Config?.ApiSecret))
                return false;

            return true;
        }

        /// <summary>
        /// Creates a PayPal order and returns the approval URL the user must be redirected to.
        /// </summary>
        /// <param name="request">Payment request details</param>
        /// <param name="returnBaseUrl">Base URL for return/cancel callbacks (e.g., "https://yourdomain.com")</param>
        public async Task<(string orderId, string approveUrl)> CreateOrderAndGetApprovalUrlAsync(PaymentRequest request, string returnBaseUrl)
        {
            ValidatePaymentRequest(request);

            if (!ValidateConfiguration())
                throw new InvalidOperationException("PayPal provider not properly configured.");

            var baseUrl = Config!.IsTestMode ? SandboxBaseUrl : ProductionBaseUrl;
            var accessToken = await GetAccessTokenAsync(baseUrl).ConfigureAwait(false);

            var (orderId, approveUrl) = await CreateOrderWithLinksAsync(baseUrl, accessToken, request, returnBaseUrl).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(approveUrl))
                throw new InvalidOperationException("PayPal did not return an approval URL.");

            return (orderId, approveUrl);
        }

        /// <summary>
        /// Captures an approved PayPal order (token from return URL) and maps to PaymentResponse.
        /// </summary>
        public async Task<PaymentResponse> CaptureApprovedOrderAsync(string orderId, PaymentRequest originalRequest)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("Order ID (token) is required for capture.", nameof(orderId));

            if (!ValidateConfiguration())
                return CreateErrorResponse("PayPal provider not properly configured.");

            try
            {
                var baseUrl = Config!.IsTestMode ? SandboxBaseUrl : ProductionBaseUrl;
                var accessToken = await GetAccessTokenAsync(baseUrl).ConfigureAwait(false);

                var captureResult = await CaptureOrderAsync(baseUrl, accessToken, orderId).ConfigureAwait(false);

                return MapCaptureToResponse(captureResult, originalRequest);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse($"An error occurred capturing the PayPal order: {ex.Message}");
            }
        }

        public override async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                // For REST redirect-based checkout, this method represents a full flow
                // for non-interactive scenarios. In the web app, prefer using
                // CreateOrderAndGetApprovalUrlAsync + CaptureApprovedOrderAsync instead.
                ValidatePaymentRequest(request);

                if (!ValidateConfiguration())
                    return CreateErrorResponse("PayPal provider not properly configured.");

                var baseUrl = Config!.IsTestMode ? SandboxBaseUrl : ProductionBaseUrl;

                var accessToken = await GetAccessTokenAsync(baseUrl).ConfigureAwait(false);
                // Use localhost fallback for non-redirect scenarios
                var (orderId, _) = await CreateOrderWithLinksAsync(baseUrl, accessToken, request, "https://localhost").ConfigureAwait(false);
                var captureResult = await CaptureOrderAsync(baseUrl, accessToken, orderId).ConfigureAwait(false);

                return MapCaptureToResponse(captureResult, request);
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
            if (string.IsNullOrWhiteSpace(transactionId))
                return CreateErrorResponse("Transaction ID is required for refund.");

            if (!ValidateConfiguration())
                return CreateErrorResponse("PayPal provider not properly configured.");

            try
            {
                var baseUrl = Config!.IsTestMode ? SandboxBaseUrl : ProductionBaseUrl;
                var accessToken = await GetAccessTokenAsync(baseUrl).ConfigureAwait(false);

                // Refund a captured payment
                var refundPayload = new Dictionary<string, object?>();
                if (amount.HasValue)
                {
                    refundPayload["amount"] = new
                    {
                        value = amount.Value.ToString("F2"),
                        currency_code = "USD" // Could be extended to use request currency
                    };
                }

                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/payments/captures/{transactionId}/refund");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                if (refundPayload.Count > 0)
                {
                    var json = JsonSerializer.Serialize(refundPayload);
                    requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                using var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return CreateErrorResponse($"PayPal refund failed: {response.StatusCode}", response.StatusCode.ToString());

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                var status = root.GetPropertyOrDefault("status")?.GetString();
                var id = root.GetPropertyOrDefault("id")?.GetString();

                var isApproved = string.Equals(status, "COMPLETED", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(status, "PENDING", StringComparison.OrdinalIgnoreCase);

                return new PaymentResponse
                {
                    IsApproved = isApproved,
                    TransactionId = id ?? string.Empty,
                    AuthorizationCode = id ?? string.Empty,
                    ResponseCode = status ?? string.Empty,
                    Message = isApproved ? "Refund processed" : "Refund failed",
                    ErrorMessage = isApproved ? null : "Refund not completed",
                    RawResponse = new Dictionary<string, string> { { "json", content } }
                };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse($"An error occurred processing the refund: {ex.Message}");
            }
        }

        private async Task<string> GetAccessTokenAsync(string baseUrl)
        {
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Config!.ApiKey}:{Config.ApiSecret}"));

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to get PayPal access token: {response.StatusCode} - {content}");

            using var doc = JsonDocument.Parse(content);
            var token = doc.RootElement.GetPropertyOrDefault("access_token")?.GetString();

            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("PayPal access token not present in response.");

            return token;
        }

        private async Task<string> CreateOrderAsync(string baseUrl, string accessToken, PaymentRequest request)
        {
            var orderPayload = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = request.CurrencyCode,
                            value = request.Amount.ToString("F2")
                        },
                        description = request.Description,
                        invoice_id = request.InvoiceNumber,
                        custom_id = request.TransactionId
                    }
                }
            };

            var json = JsonSerializer.Serialize(orderPayload);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"PayPal create order failed: {response.StatusCode} - {content}");

            using var doc = JsonDocument.Parse(content);
            var id = doc.RootElement.GetPropertyOrDefault("id")?.GetString();

            if (string.IsNullOrWhiteSpace(id))
                throw new Exception("PayPal order id not present in response.");

            return id;
        }

        private async Task<JsonDocument> CaptureOrderAsync(string baseUrl, string accessToken, string orderId)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders/{orderId}/capture");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            // PayPal expects a JSON payload; send an empty object
            httpRequest.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"PayPal capture order failed: {response.StatusCode} - {content}");

            return JsonDocument.Parse(content);
        }

        private async Task<(string orderId, string approveUrl)> CreateOrderWithLinksAsync(string baseUrl, string accessToken, PaymentRequest request, string returnBaseUrl)
        {
            var orderPayload = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = request.CurrencyCode,
                            value = request.Amount.ToString("F2")
                        },
                        description = request.Description,
                        invoice_id = request.InvoiceNumber,
                        custom_id = request.TransactionId
                    }
                },
                application_context = new
                {
                    brand_name = request.Description ?? "Checkout",
                    user_action = "PAY_NOW",
                    return_url = $"{returnBaseUrl}/Store/PayPalReturn",
                    cancel_url = $"{returnBaseUrl}/Store/Checkout"
                }
            };

            var json = JsonSerializer.Serialize(orderPayload);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"PayPal create order failed: {response.StatusCode} - {content}");

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            var id = root.GetPropertyOrDefault("id")?.GetString();

            if (string.IsNullOrWhiteSpace(id))
                throw new Exception("PayPal order id not present in response.");

            string? approveUrl = null;
            if (root.TryGetProperty("links", out var linksEl) && linksEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var link in linksEl.EnumerateArray())
                {
                    var rel = link.GetPropertyOrDefault("rel")?.GetString();
                    if (string.Equals(rel, "approve", StringComparison.OrdinalIgnoreCase))
                    {
                        approveUrl = link.GetPropertyOrDefault("href")?.GetString();
                        break;
                    }
                }
            }

            return (id, approveUrl ?? string.Empty);
        }

        private PaymentResponse MapCaptureToResponse(JsonDocument captureDoc, PaymentRequest request)
        {
            var root = captureDoc.RootElement;

            var status = root.GetPropertyOrDefault("status")?.GetString();
            var id = root.GetPropertyOrDefault("id")?.GetString();

            // Navigate to first capture id if available
            string? captureId = null;
            if (root.TryGetProperty("purchase_units", out var purchaseUnits) &&
                purchaseUnits.ValueKind == JsonValueKind.Array &&
                purchaseUnits.GetArrayLength() > 0)
            {
                var pu = purchaseUnits[0];
                if (pu.TryGetProperty("payments", out var payments) &&
                    payments.TryGetProperty("captures", out var captures) &&
                    captures.ValueKind == JsonValueKind.Array &&
                    captures.GetArrayLength() > 0)
                {
                    var cap = captures[0];
                    captureId = cap.GetPropertyOrDefault("id")?.GetString();
                    status ??= cap.GetPropertyOrDefault("status")?.GetString();
                }
            }

            var isApproved = string.Equals(status, "COMPLETED", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(status, "APPROVED", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(status, "PENDING", StringComparison.OrdinalIgnoreCase);

            var json = captureDoc.RootElement.GetRawText();

            return new PaymentResponse
            {
                IsApproved = isApproved,
                TransactionId = captureId ?? id ?? string.Empty,
                AuthorizationCode = captureId ?? id ?? string.Empty,
                ResponseCode = status ?? string.Empty,
                Message = isApproved ? "Payment processed successfully" : "Payment not completed",
                ErrorMessage = isApproved ? null : "PayPal did not return a completed status.",
                RawResponse = new Dictionary<string, string> { { "json", json } }
            };
        }
    }

    internal static class JsonExtensions
    {
        public static JsonElement? GetPropertyOrDefault(this JsonElement element, string name)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value))
                return value;
            return null;
        }
    }
}
