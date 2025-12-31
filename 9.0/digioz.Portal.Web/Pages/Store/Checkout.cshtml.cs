using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.PaymentProviders.Abstractions;
using digioz.Portal.PaymentProviders.Models;
using digioz.Portal.Web.Services;
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
        private readonly IPayPalRedirectService _payPalRedirectService;
        private readonly ILogService _logService;
        private readonly ILogger<CheckoutModel> _logger;

        public CheckoutModel(
            IShoppingCartService cartService,
            IProductService productService,
            IOrderService orderService,
            IOrderDetailService orderDetailService,
            IConfigService configService,
            IPaymentProviderFactory paymentProviderFactory,
            IPayPalRedirectService payPalRedirectService,
            ILogService logService,
            ILogger<CheckoutModel> logger) {
            _cartService = cartService;
            _productService = productService;
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _configService = configService;
            _paymentProviderFactory = paymentProviderFactory;
            _payPalRedirectService = payPalRedirectService;
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
        
        public string PaymentProvider { get; set; } = string.Empty;
        public bool IsPayPal => string.Equals(PaymentProvider, "PayPal", StringComparison.OrdinalIgnoreCase);

        public void OnGet() {
            LoadCartItems();
            PaymentProvider = GetConfiguredPaymentProvider();
            
            _logger.LogInformation("Checkout OnGet: PaymentProvider = '{PaymentProvider}'", PaymentProvider);

            // Pre-populate email from the authenticated user if available
            if (string.IsNullOrWhiteSpace(Order.Email))
            {
                var emailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                Order.Email = emailClaim ?? User.Identity?.Name ?? string.Empty;
            }
        }

        public async Task<IActionResult> OnPostAsync() {
            LoadCartItems();
            PaymentProvider = GetConfiguredPaymentProvider();
            
            _logger.LogInformation("Checkout OnPostAsync: PaymentProvider = '{PaymentProvider}', IsPayPal = {IsPayPal}", 
                PaymentProvider, IsPayPal);

            // For PayPal, clear any model state errors related to credit card fields
            if (IsPayPal)
            {
                _logger.LogInformation("Removing credit card validation errors for PayPal");
                ModelState.Remove("Order.Ccnumber");
                ModelState.Remove("Order.Ccexp");
                ModelState.Remove("Order.CccardCode");
            }

            // Ensure email is populated on POST from user claims if missing
            if (string.IsNullOrWhiteSpace(Order.Email))
            {
                var emailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                Order.Email = emailClaim ?? User.Identity?.Name ?? string.Empty;
            }

            if (!ModelState.IsValid || !CartItems.Any()) {
                _logger.LogWarning("ModelState invalid or cart empty. ModelState.IsValid={IsValid}, CartItems.Count={Count}", 
                    ModelState.IsValid, CartItems.Count);
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

                // Get configured provider
                var paymentProviderName = GetConfiguredPaymentProvider();
                
                _logger.LogInformation("Payment processing: providerName = '{ProviderName}', checking if PayPal", paymentProviderName);
                
                if (string.IsNullOrEmpty(paymentProviderName)) {
                    _logger.LogError("Payment provider name is null or empty");
                    ModelState.AddModelError("", "Payment provider is not configured. Please contact support.");
                    return Page();
                }

                // Check if this is PayPal: use redirect flow
                if (string.Equals(paymentProviderName, "PayPal", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Using PayPal redirect flow for order {OrderId}", Order.Id);
                    return await ProcessPayPalRedirectAsync(userId);
                }

                // Otherwise use direct card processing (e.g., Authorize.Net)
                _logger.LogInformation("Using direct payment with provider '{Provider}' for order {OrderId}", paymentProviderName, Order.Id);
                
                // Store the full credit card number temporarily for payment processing
                var fullCardNumber = Order.Ccnumber;
                var fullCvv = Order.CccardCode;
                
                var paymentResponse = await ProcessPaymentAsync(paymentProviderName);

                // Mask sensitive credit card data before saving to database
                MaskCreditCardData();
                
                Order.TrxApproved = paymentResponse.IsApproved;
                Order.TrxId = paymentResponse.TransactionId;
                Order.TrxAuthorizationCode = paymentResponse.AuthorizationCode;
                Order.TrxMessage = paymentResponse.Message;
                Order.TrxResponseCode = paymentResponse.ResponseCode;

                if (!paymentResponse.IsApproved) {
                    ModelState.AddModelError("", $"Payment failed: {paymentResponse.ErrorMessage}");
                    _logger.LogWarning("Payment declined for user {UserId}: {ErrorCode} - {ErrorMessage}",
                        userId, paymentResponse.ErrorCode, paymentResponse.ErrorMessage);
                    
                    LogPaymentFailure(userId, Order.Id, paymentProviderName, paymentResponse);
                    
                    return Page();
                }

                // Save order
                _orderService.Add(Order);

                // Create order details and clear cart
                await CreateOrderDetailsAndClearCart(userId);

                return RedirectToPage("OrderConfirmation", new { orderId = Order.Id });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error processing checkout");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                
                if (!string.IsNullOrEmpty(userId))
                {
                    LogCheckoutError(ex);
                }
                
                return Page();
            }
        }

        private async Task<IActionResult> ProcessPayPalRedirectAsync(string userId)
        {
            try
            {
                _logger.LogInformation("ProcessPayPalRedirectAsync: Starting PayPal order creation for user {UserId}", userId);
                
                // Save a pending order record in the database BEFORE redirecting to PayPal
                // This ensures we can retrieve it when PayPal redirects back
                Order.TrxApproved = false; // Mark as pending
                Order.TrxMessage = "Pending PayPal approval";
                Order.TrxResponseCode = "PENDING";

                // Mask any credit card data (PayPal doesn't use credit cards, but mask any dummy values)
                MaskCreditCardData();

                _logger.LogInformation("ProcessPayPalRedirectAsync: Saving pending order {OrderId}", Order.Id);
                _orderService.Add(Order);

                // Build payment request for PayPal order creation
                var request = BuildPaymentRequest();

                _logger.LogInformation("ProcessPayPalRedirectAsync: Creating PayPal order for {OrderId}, Amount={Amount}", 
                    Order.Id, request.Amount);

                // Get current base URL
                var returnBaseUrl = $"{Request.Scheme}://{Request.Host}";
                _logger.LogInformation("ProcessPayPalRedirectAsync: Return base URL = {ReturnBaseUrl}", returnBaseUrl);

                // Create PayPal order and get approval URL
                var (paypalOrderId, approveUrl) = await _payPalRedirectService.CreateOrderAsync(request, returnBaseUrl);

                _logger.LogInformation("ProcessPayPalRedirectAsync: PayPal order created successfully. PayPalOrderId={PayPalOrderId}, ApproveUrl={ApproveUrl}", 
                    paypalOrderId, approveUrl);

                // Store PayPal order ID in the pending order record
                Order.TrxId = paypalOrderId;
                _orderService.Update(Order);

                _logger.LogInformation("ProcessPayPalRedirectAsync: Redirecting to PayPal approval URL");
                
                // Redirect to PayPal for approval (order context is now in database)
                return Redirect(approveUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessPayPalRedirectAsync: Error creating PayPal order. Exception: {Message}, StackTrace: {StackTrace}", 
                    ex.Message, ex.StackTrace);
                
                // Try to get inner exception details
                if (ex.InnerException != null)
                {
                    _logger.LogError("ProcessPayPalRedirectAsync: Inner exception: {InnerMessage}", ex.InnerException.Message);
                }
                
                ModelState.AddModelError("", $"Failed to initiate PayPal payment: {ex.Message}");
                return Page();
            }
        }

        private async Task<PaymentResponse> ProcessPaymentAsync(string paymentProviderName) {
            try {
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

                var provider = _paymentProviderFactory.CreateProvider(paymentProviderName);

                var request = BuildPaymentRequest();

                _logger.LogInformation("Processing payment via {Provider} for order {OrderId}: Amount={Amount}", 
                    paymentProviderName, Order.Id, CartTotal);

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

        private PaymentRequest BuildPaymentRequest()
        {
            // Parse credit card expiration
            var expirationParts = Order.Ccexp?.Split('/') ?? new[] { "", "" };
            var expMonth = expirationParts.Length > 0 ? expirationParts[0].Trim() : "01";
            var expYear = expirationParts.Length > 1 ? expirationParts[1].Trim() : "25";

            return new PaymentRequest {
                TransactionId = Order.Id,
                Amount = CartTotal, // For PayPal REST this is decimal dollars, not cents
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
        }

        private async Task CreateOrderDetailsAndClearCart(string userId)
        {
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

        private string GetConfiguredPaymentProvider() {
            var config = _configService.GetByKey("PaymentProvider");
            if (config != null && !string.IsNullOrEmpty(config.ConfigValue)) {
                var value = config.ConfigValue.Trim();
                _logger.LogInformation("GetConfiguredPaymentProvider: Found config value = '{Value}'", value);
                return value;
            }

            _logger.LogWarning("GetConfiguredPaymentProvider: PaymentProvider configuration key not found in settings");
            return string.Empty;
        }

        private string GenerateInvoiceNumber() {
            var timestamp = DateTime.Now.ToString("yyMMddHHmm");
            var guidSegment = Guid.NewGuid().ToString("N").Substring(0, 5).ToUpperInvariant();
            return $"INV-{timestamp}-{guidSegment}";
        }

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
                _logger.LogError(ex, "Failed to log payment failure to database");
            }
        }

        private void LogCheckoutError(Exception ex)
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

        private void MaskCreditCardData()
        {
            // Only save last 4 digits of credit card for security
            if (!string.IsNullOrEmpty(Order.Ccnumber))
            {
                var cardNumber = Order.Ccnumber.Replace(" ", "").Replace("-", "");
                if (cardNumber.Length >= 4)
                {
                    var last4 = cardNumber.Substring(cardNumber.Length - 4);
                    Order.Ccnumber = $"****{last4}";
                    _logger.LogInformation("Credit card number masked for order {OrderId}", Order.Id);
                }
                else
                {
                    Order.Ccnumber = "****";
                }
            }

            // Remove CVV completely - never store this
            if (!string.IsNullOrEmpty(Order.CccardCode))
            {
                Order.CccardCode = "***";
                _logger.LogInformation("CVV masked for order {OrderId}", Order.Id);
            }
        }
    }
}
