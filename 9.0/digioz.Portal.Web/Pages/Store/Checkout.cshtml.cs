using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.PaymentProviders.Abstractions;
using digioz.Portal.PaymentProviders.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Pages.Store {
    [Authorize]
    public class CheckoutModel : PageModel {
        private readonly IShoppingCartService _cartService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IConfigService _configService;
        private readonly IPaymentProviderFactory _paymentProviderFactory;
        private readonly ILogService _logService;
        private readonly ILogger<CheckoutModel> _logger;

        public CheckoutModel(
            IShoppingCartService cartService,
            IProductService productService,
            IOrderService orderService,
            IOrderDetailService orderDetailService,
            IConfigService configService,
            IPaymentProviderFactory paymentProviderFactory,
            ILogService logService,
            ILogger<CheckoutModel> logger) {
            _cartService = cartService;
            _productService = productService;
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _configService = configService;
            _paymentProviderFactory = paymentProviderFactory;
            _logService = logService;
            _logger = logger;
        }

        public class CartItemViewModel {
            public string? ProductId { get; set; }
            public string? ProductName { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }

        [BindProperty]
        public Order Order { get; set; } = new();

        public List<CartItemViewModel> CartItems { get; set; } = new();
        public decimal CartTotal { get; set; }

        public void OnGet() {
            LoadCartItems();

            // Pre-populate email from the authenticated user if available
            if (string.IsNullOrWhiteSpace(Order.Email))
            {
                var emailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                Order.Email = emailClaim ?? User.Identity?.Name ?? string.Empty;
            }
        }

        public async Task<IActionResult> OnPostAsync() {
            LoadCartItems();

            // Ensure email is populated on POST from user claims if missing
            if (string.IsNullOrWhiteSpace(Order.Email))
            {
                var emailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                Order.Email = emailClaim ?? User.Identity?.Name ?? string.Empty;
            }

            if (!ModelState.IsValid || !CartItems.Any()) {
                return Page();
            }

            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            
            try {
                if (string.IsNullOrEmpty(userId)) {
                    return RedirectToPage("/Identity/Account/Login");
                }

                // If same as billing, copy billing to shipping
                if (string.IsNullOrEmpty(Order.ShippingAddress)) {
                    Order.ShippingAddress = Order.BillingAddress;
                    Order.ShippingAddress2 = Order.BillingAddress2;
                    Order.ShippingCity = Order.BillingCity;
                    Order.ShippingState = Order.BillingState;
                    Order.ShippingZip = Order.BillingZip;
                    Order.ShippingCountry = Order.BillingCountry;
                }

                // Set order properties
                Order.Id = Guid.NewGuid().ToString();
                Order.UserId = userId;
                Order.OrderDate = DateTime.Now;
                Order.InvoiceNumber = GenerateInvoiceNumber();
                Order.Total = CartTotal;
                Order.Ccamount = CartTotal;

                // Ensure required contact fields are populated before saving
                if (string.IsNullOrWhiteSpace(Order.Email))
                {
                    // Fall back to the currently authenticated user's name if email was not bound
                    Order.Email = User.Identity?.Name ?? string.Empty;
                }

                // Process payment using administrator-configured payment provider
                var paymentResponse = await ProcessPaymentAsync();

                Order.TrxApproved = paymentResponse.IsApproved;
                Order.TrxId = paymentResponse.TransactionId;
                Order.TrxAuthorizationCode = paymentResponse.AuthorizationCode;
                Order.TrxMessage = paymentResponse.Message;
                Order.TrxResponseCode = paymentResponse.ResponseCode;

                if (!paymentResponse.IsApproved) {
                    ModelState.AddModelError("", $"Payment failed: {paymentResponse.ErrorMessage}");
                    _logger.LogWarning("Payment declined for user {UserId}: {ErrorCode} - {ErrorMessage}",
                        userId, paymentResponse.ErrorCode, paymentResponse.ErrorMessage);
                    
                    // Get provider name for logging
                    var providerName = GetConfiguredPaymentProvider();
                    
                    // Log payment failure to database
                    LogPaymentFailure(userId, Order.Id, providerName, paymentResponse);
                    
                    return Page();
                }

                // Save order
                _orderService.Add(Order);

                // Create order details and clear cart
                var cartItems = _cartService.GetAll()
                    .Where(c => c.UserId == userId)
                    .ToList();

                foreach (var cartItem in cartItems) {
                    var product = _productService.Get(cartItem.ProductId);
                    if (product != null) {
                        var detail = new OrderDetail {
                            Id = Guid.NewGuid().ToString(),
                            OrderId = Order.Id,
                            ProductId = cartItem.ProductId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = product.Price,
                            Description = product.Name
                        };
                        _orderDetailService.Add(detail);
                    }

                    _cartService.Delete(cartItem.Id);
                }

                return RedirectToPage("OrderConfirmation", new { orderId = Order.Id });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error processing checkout");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                
                // Log checkout error to database
                if (!string.IsNullOrEmpty(userId))
                {
                    var logEntry = new Log
                    {
                        Message = $"Error during checkout processing",
                        Level = "Error",
                        Exception = ex.ToString(),
                        LogEvent = "PaymentError",
                        Timestamp = DateTime.UtcNow
                    };
                    try
                    {
                        _logService.Add(logEntry);
                    }
                    catch (Exception logEx)
                    {
                        _logger.LogError(logEx, "Failed to log checkout error to database");
                    }
                }
                
                return Page();
            }
        }

        private void LoadCartItems() {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            if (string.IsNullOrEmpty(userId))
                return;

            var cartItems = _cartService.GetAll()
                .Where(c => c.UserId == userId)
                .ToList();

            CartTotal = 0;

            foreach (var item in cartItems) {
                var product = _productService.Get(item.ProductId);
                if (product != null) {
                    CartItems.Add(new CartItemViewModel {
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = item.Quantity
                    });

                    CartTotal += product.Price * item.Quantity;
                }
            }
        }

        private async Task<PaymentResponse> ProcessPaymentAsync() {
            try {
                // Get the configured payment provider from settings
                var paymentProviderName = GetConfiguredPaymentProvider();
                
                if (string.IsNullOrEmpty(paymentProviderName)) {
                    _logger.LogError("No payment provider configured in settings");
                    return new PaymentResponse {
                        IsApproved = false,
                        ErrorMessage = "Payment provider is not configured. Please contact support.",
                        ErrorCode = "PROVIDER_NOT_CONFIGURED"
                    };
                }

                // Check if payment gateway is available
                if (!_paymentProviderFactory.IsProviderAvailable(paymentProviderName)) {
                    _logger.LogError("Payment provider {Provider} is not available. Available providers: {AvailableProviders}",
                        paymentProviderName, string.Join(", ", _paymentProviderFactory.GetAvailableProviders()));
                    return new PaymentResponse {
                        IsApproved = false,
                        ErrorMessage = $"Payment provider '{paymentProviderName}' is not configured",
                        ErrorCode = "PROVIDER_NOT_AVAILABLE"
                    };
                }

                // Create provider instance - pass null to use the factory's internal service provider
                // which was properly configured with HttpClient in Program.cs
                var provider = _paymentProviderFactory.CreateProvider(paymentProviderName);

                // Parse credit card expiration
                var expirationParts = Order.Ccexp?.Split('/') ?? new[] { "", "" };
                var expMonth = expirationParts.Length > 0 ? expirationParts[0].Trim() : "01";
                var expYear = expirationParts.Length > 1 ? expirationParts[1].Trim() : "25";

                // Build payment request
                var request = new PaymentRequest {
                    TransactionId = Order.Id,
                    Amount = (long)(CartTotal * 100), // Convert to cents
                    CurrencyCode = "USD",
                    CardNumber = Order.Ccnumber,
                    ExpirationMonth = expMonth,
                    ExpirationYear = expYear,
                    CardCode = Order.CccardCode,
                    CardholderName = $"{Order.FirstName} {Order.LastName}",
                    CustomerEmail = Order.Email,
                    CustomerPhone = Order.Phone,
                    BillingAddress = Order.BillingAddress,
                    BillingCity = Order.BillingCity,
                    BillingState = Order.BillingState,
                    BillingZip = Order.BillingZip,
                    BillingCountry = Order.BillingCountry,
                    ShippingAddress = Order.ShippingAddress,
                    ShippingCity = Order.ShippingCity,
                    ShippingState = Order.ShippingState,
                    ShippingZip = Order.ShippingZip,
                    ShippingCountry = Order.ShippingCountry,
                    InvoiceNumber = Order.InvoiceNumber,
                    Description = "Portal Store Purchase"
                };

                _logger.LogInformation("Processing payment via {Provider} for order {OrderId}: Amount={Amount}", 
                    paymentProviderName, Order.Id, CartTotal);

                // Process payment
                var response = await provider.ProcessPaymentAsync(request);

                _logger.LogInformation("Payment processed via {Provider} for order {OrderId}: Approved={IsApproved}, ResponseCode={ResponseCode}",
                    paymentProviderName, Order.Id, response.IsApproved, response.ResponseCode);

                return response;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error processing payment for order {OrderId}", Order.Id);
                return new PaymentResponse {
                    IsApproved = false,
                    ErrorMessage = $"An error occurred processing your payment: {ex.Message}",
                    ErrorCode = "PAYMENT_ERROR"
                };
            }
        }

        /// <summary>
        /// Gets the configured payment provider name from application settings.
        /// The administrator determines which single payment provider is used for all transactions.
        /// </summary>
        /// <returns>The configured payment provider name (e.g., "AuthorizeNet", "PayPal"), or null if not configured</returns>
        private string GetConfiguredPaymentProvider() {
            // Check for payment provider configuration in app settings
            var config = _configService.GetByKey("PaymentProvider");
            if (config != null && !string.IsNullOrEmpty(config.ConfigValue)) {
                return config.ConfigValue.Trim();
            }

            // Log if no configuration found
            _logger.LogWarning("PaymentProvider configuration key not found in settings");
            return string.Empty;
        }

        private string GenerateInvoiceNumber() {
            // DB column supports max 20 characters, so keep invoice number short
            // Format: INV-{yyMMddHHmm}-{8-char GUID segment} (total 3+1+10+1+5 = 20)
            var timestamp = DateTime.Now.ToString("yyMMddHHmm");
            var guidSegment = Guid.NewGuid().ToString("N").Substring(0, 5).ToUpperInvariant();
            return $"INV-{timestamp}-{guidSegment}";
        }

        /// <summary>
        /// Logs payment failure details to the database Log table.
        /// </summary>
        private void LogPaymentFailure(string userId, string orderId, string provider, PaymentResponse response)
        {
            try
            {
                var exceptionDetails = new System.Text.StringBuilder();
                exceptionDetails.AppendLine($"Order ID: {orderId}");
                exceptionDetails.AppendLine($"Provider: {provider}");
                exceptionDetails.AppendLine($"Error Code: {response.ErrorCode}");
                exceptionDetails.AppendLine($"Error Message: {response.ErrorMessage}");
                exceptionDetails.AppendLine($"Response Code: {response.ResponseCode}");
                exceptionDetails.AppendLine($"Response Message: {response.Message}");

                var logEntry = new Log
                {
                    Message = $"Payment declined for order {orderId}: {response.ErrorMessage}",
                    Level = "Warning",
                    Exception = exceptionDetails.ToString(),
                    LogEvent = "PaymentDeclined",
                    Timestamp = DateTime.UtcNow
                };

                _logService.Add(logEntry);
            }
            catch (Exception ex)
            {
                // Log the logging failure to ILogger only to avoid infinite recursion
                _logger.LogError(ex, "Failed to log payment failure to database");
            }
        }
    }
}
