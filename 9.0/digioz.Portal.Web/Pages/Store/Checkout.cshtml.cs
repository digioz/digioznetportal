using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Pages.Store {
    [Authorize]
    public class CheckoutModel : PageModel {
        private readonly IShoppingCartService _cartService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IConfigService _configService;

        public CheckoutModel(
            IShoppingCartService cartService,
            IProductService productService,
            IOrderService orderService,
            IOrderDetailService orderDetailService,
            IConfigService configService) {
            _cartService = cartService;
            _productService = productService;
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _configService = configService;
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
        }

        public async Task<IActionResult> OnPostAsync() {
            LoadCartItems();

            if (!ModelState.IsValid || !CartItems.Any()) {
                return Page();
            }

            try {
                var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
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

                // Process payment using PaymentProviders
                bool paymentApproved = await ProcessPaymentAsync();

                Order.TrxApproved = paymentApproved;

                if (!paymentApproved) {
                    ModelState.AddModelError("", "Payment failed. Please check your card information and try again.");
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
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
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

        private async Task<bool> ProcessPaymentAsync() {
            try {
                // Get payment provider type from config
                var configData = _configService.GetByKey("PaymentProviderType");
                if (configData == null || string.IsNullOrEmpty(configData.ConfigValue)) {
                    return false;
                }

                // Determine payment provider and process
                if (configData.ConfigValue == "Stripe") {
                    // Implement Stripe payment
                    return await ProcessStripePaymentAsync();
                } else if (configData.ConfigValue == "PayPal") {
                    // Implement PayPal payment
                    return await ProcessPayPalPaymentAsync();
                } else if (configData.ConfigValue == "Square") {
                    // Implement Square payment
                    return await ProcessSquarePaymentAsync();
                }

                return false;
            } catch {
                return false;
            }
        }

        private async Task<bool> ProcessStripePaymentAsync() {
            // TODO: Implement Stripe payment processing
            // Use digioz.Portal.PaymentProviders library
            return true; // Placeholder
        }

        private async Task<bool> ProcessPayPalPaymentAsync() {
            // TODO: Implement PayPal payment processing
            // Use digioz.Portal.PaymentProviders library
            return true; // Placeholder
        }

        private async Task<bool> ProcessSquarePaymentAsync() {
            // TODO: Implement Square payment processing
            // Use digioz.Portal.PaymentProviders library
            return true; // Placeholder
        }

        private string GenerateInvoiceNumber() {
            return $"INV-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}