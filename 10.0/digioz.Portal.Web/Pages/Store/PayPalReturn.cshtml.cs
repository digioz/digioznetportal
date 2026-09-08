using System;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.PaymentProviders.Models;
using digioz.Portal.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Pages.Store
{
    [Authorize]
    public class PayPalReturnModel : PageModel
    {
        private readonly IShoppingCartService _cartService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IPayPalRedirectService _payPalRedirectService;
        private readonly ILogService _logService;
        private readonly ILogger<PayPalReturnModel> _logger;

        public PayPalReturnModel(
            IShoppingCartService cartService,
            IProductService productService,
            IOrderService orderService,
            IOrderDetailService orderDetailService,
            IPayPalRedirectService payPalRedirectService,
            ILogService logService,
            ILogger<PayPalReturnModel> logger)
        {
            _cartService = cartService;
            _productService = productService;
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _payPalRedirectService = payPalRedirectService;
            _logService = logService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("PayPal return called without token");
                TempData["Error"] = "Invalid PayPal return. Please try again.";
                return RedirectToPage("/Store/Index");
            }

            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("PayPal return: user not authenticated");
                return RedirectToPage("/Identity/Account/Login");
            }

            try
            {
                // Find the pending order by PayPal order ID (stored in TrxId)
                var order = _orderService.GetByTokenAndUserId(token, userId, "PENDING");

                if (order == null)
                {
                    _logger.LogWarning("PayPal return: no pending order found for token {Token} and user {UserId}", token, userId);
                    TempData["Error"] = "Order not found or already processed.";
                    return RedirectToPage("/Store/Index");
                }

                _logger.LogInformation("Capturing PayPal order {PayPalOrderId} for order {OrderId}", token, order.Id);

                // Build minimal PaymentRequest for capture response mapping
                var request = new PaymentRequest
                {
                    TransactionId = order.Id,
                    Amount = order.Total,
                    CurrencyCode = "USD",
                    InvoiceNumber = order.InvoiceNumber,
                    Description = "Portal Store Purchase"
                };

                // Capture the approved PayPal order
                var paymentResponse = await _payPalRedirectService.CaptureOrderAsync(token, request);

                if (!paymentResponse.IsApproved)
                {
                    _logger.LogWarning("PayPal capture failed for order {OrderId}: {ErrorMessage}", 
                        order.Id, paymentResponse.ErrorMessage);

                    // Update order with failure
                    order.TrxApproved = false;
                    order.TrxMessage = paymentResponse.ErrorMessage ?? paymentResponse.Message;
                    order.TrxResponseCode = paymentResponse.ResponseCode;
                    _orderService.Update(order);

                    LogPaymentFailure(userId, order.Id, "PayPal", paymentResponse);

                    TempData["Error"] = $"Payment failed: {paymentResponse.ErrorMessage}";
                    return RedirectToPage("/Store/Checkout");
                }

                // Update order with success
                order.TrxApproved = true;
                order.TrxId = paymentResponse.TransactionId;
                order.TrxAuthorizationCode = paymentResponse.AuthorizationCode;
                order.TrxMessage = paymentResponse.Message;
                order.TrxResponseCode = paymentResponse.ResponseCode;
                _orderService.Update(order);

                // Get cart items for order details
                var cartItems = _cartService.GetAll()
                    .Where(c => c.UserId == userId)
                    .ToList();

                if (!cartItems.Any())
                {
                    _logger.LogWarning("No cart items found for user {UserId} during PayPal return", userId);
                    TempData["Error"] = "Your cart is empty. The order could not be completed.";
                    return RedirectToPage("/Store/Index");
                }

                // Create order details
                foreach (var cartItem in cartItems)
                {
                    var product = _productService.Get(cartItem.ProductId);
                    if (product != null)
                    {
                        var detail = new OrderDetail
                        {
                            Id = Guid.NewGuid().ToString(),
                            OrderId = order.Id,
                            ProductId = cartItem.ProductId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = product.Price,
                            Description = product.Name
                        };
                        _orderDetailService.Add(detail);
                    }

                    _cartService.Delete(cartItem.Id);
                }

                _logger.LogInformation("PayPal order {OrderId} completed successfully", order.Id);

                return RedirectToPage("OrderConfirmation", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing PayPal payment for token {Token}", token);

                var order = _orderService.GetByTokenAndUserId(token, userId);
                var orderId = order?.Id ?? "Unknown";

                LogCheckoutError(ex, orderId);

                TempData["Error"] = "An error occurred processing your payment. Please contact support.";
                return RedirectToPage("/Store/Index");
            }
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

        private void LogCheckoutError(Exception ex, string orderId)
        {
            var logEntry = new Log
            {
                Message = $"Error during PayPal return processing for order {orderId}",
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
    }
}
