# PayPal REST API Integration Guide

This guide covers the PayPal payment provider implementation using the REST API v2 (Orders API) with redirect-based approval flow.

## Overview

The PayPal provider uses a **redirect-based checkout flow**:

1. **Create Order**: Server creates a PayPal order
2. **Redirect**: User is redirected to PayPal to approve the payment
3. **Return**: PayPal redirects back to your site
4. **Capture**: Server captures the approved payment

This differs from direct card processing (like Authorize.Net) where everything happens server-side.

## Configuration

### Get PayPal Credentials

1. Go to https://developer.paypal.com
2. Log in to your PayPal account
3. Navigate to **Dashboard** ? **My Apps & Credentials**
4. Under **REST API apps**, create a new app or use existing
5. Copy **Client ID** and **Secret**

### Sandbox vs. Production

- **Sandbox**: For testing (use sandbox credentials)
  - API Base: `https://api-m.sandbox.paypal.com`
  - Create sandbox buyer/seller accounts in developer dashboard
  
- **Production**: For live payments (use production credentials)
  - API Base: `https://api-m.paypal.com`
  - Use real PayPal accounts

### appsettings.json

```json
{
  "PaymentProviders": {
    "PayPal": {
      "ClientId": "YOUR_CLIENT_ID",
      "ClientSecret": "YOUR_CLIENT_SECRET",
      "IsTestMode": true
    }
  }
}
```

For production (`appsettings.Production.json`):

```json
{
  "PaymentProviders": {
    "PayPal": {
      "ClientId": "YOUR_PRODUCTION_CLIENT_ID",
      "ClientSecret": "YOUR_PRODUCTION_CLIENT_SECRET",
      "IsTestMode": false
    }
  }
}
```

## Implementation

### 1. Register Services (Program.cs)

```csharp
using digioz.Portal.PaymentProviders.DependencyInjection;
using digioz.Portal.Web.Services;

// Configure HttpClient for PayPalProvider
builder.Services.AddHttpClient<digioz.Portal.PaymentProviders.Providers.PayPalProvider>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });

// Add payment providers
builder.Services.AddPaymentProviders(paymentProviderBuilder =>
{
    var paypalConfig = builder.Configuration.GetSection("PaymentProviders:PayPal");
    if (paypalConfig.Exists())
    {
        paymentProviderBuilder.ConfigureProvider("PayPal", config =>
        {
            config.ApiKey = paypalConfig["ClientId"];       // ClientId
            config.ApiSecret = paypalConfig["ClientSecret"]; // ClientSecret
            config.IsTestMode = paypalConfig.GetValue<bool>("IsTestMode");
        });
    }
});

// Register PayPal redirect service
builder.Services.AddScoped<IPayPalRedirectService, PayPalRedirectService>();
```

### 2. Create PayPal Redirect Service

Create `Services/IPayPalRedirectService.cs`:

```csharp
using digioz.Portal.PaymentProviders.Abstractions;
using digioz.Portal.PaymentProviders.Models;
using digioz.Portal.PaymentProviders.Providers;

namespace digioz.Portal.Web.Services
{
    public interface IPayPalRedirectService
    {
        Task<(string orderId, string approveUrl)> CreateOrderAsync(
            PaymentRequest request, 
            string returnBaseUrl);
            
        Task<PaymentResponse> CaptureOrderAsync(
            string paypalOrderId, 
            PaymentRequest originalRequest);
    }

    public class PayPalRedirectService : IPayPalRedirectService
    {
        private readonly IPaymentProviderFactory _factory;

        public PayPalRedirectService(IPaymentProviderFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task<(string orderId, string approveUrl)> CreateOrderAsync(
            PaymentRequest request, 
            string returnBaseUrl)
        {
            var provider = ResolveProvider();
            return await provider.CreateOrderAndGetApprovalUrlAsync(request, returnBaseUrl);
        }

        public async Task<PaymentResponse> CaptureOrderAsync(
            string paypalOrderId, 
            PaymentRequest originalRequest)
        {
            var provider = ResolveProvider();
            return await provider.CaptureApprovedOrderAsync(paypalOrderId, originalRequest);
        }

        private PayPalProvider ResolveProvider()
        {
            if (!_factory.IsProviderAvailable("PayPal"))
                throw new InvalidOperationException("PayPal provider is not available.");

            var provider = _factory.CreateProvider("PayPal") as PayPalProvider;
            if (provider == null)
                throw new InvalidOperationException("Failed to resolve PayPal provider instance.");

            return provider;
        }
    }
}
```

### 3. Update Checkout Page Model

Update your `Checkout.cshtml.cs`:

```csharp
using digioz.Portal.PaymentProviders.Models;
using digioz.Portal.Web.Services;

[Authorize]
public class CheckoutModel : PageModel
{
    private readonly IPayPalRedirectService _payPalRedirectService;
    private readonly IOrderService _orderService;
    private readonly IConfigService _configService;

    [BindProperty]
    public Order Order { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // Set order properties
        Order.Id = Guid.NewGuid().ToString();
        Order.UserId = User.FindFirst("sub")?.Value;
        Order.OrderDate = DateTime.Now;
        Order.InvoiceNumber = GenerateInvoiceNumber();
        Order.Total = CalculateTotal();

        var paymentProvider = GetConfiguredPaymentProvider(); // From config/database

        if (string.Equals(paymentProvider, "PayPal", StringComparison.OrdinalIgnoreCase))
        {
            return await ProcessPayPalRedirectAsync();
        }
        
        // Handle other providers...
        return Page();
    }

    private async Task<IActionResult> ProcessPayPalRedirectAsync()
    {
        try
        {
            // IMPORTANT: Save pending order BEFORE redirect
            // TempData is lost during external redirect
            Order.TrxApproved = false;
            Order.TrxResponseCode = "PENDING";
            Order.TrxMessage = "Awaiting PayPal approval";
            _orderService.Add(Order);

            // Build payment request
            var request = new PaymentRequest
            {
                TransactionId = Order.Id,
                Amount = Order.Total, // Decimal dollars (e.g., 100.50)
                CurrencyCode = "USD",
                InvoiceNumber = Order.InvoiceNumber,
                Description = "Portal Store Purchase"
            };

            // Get dynamic base URL (works in dev and production)
            var returnBaseUrl = $"{Request.Scheme}://{Request.Host}";
            // Dev:  https://localhost:7215
            // Prod: https://yourdomain.com

            // Create PayPal order and get approval URL
            var (paypalOrderId, approveUrl) = await _payPalRedirectService
                .CreateOrderAsync(request, returnBaseUrl);

            _logger.LogInformation("PayPal order created: {PayPalOrderId}, redirecting to {ApproveUrl}", 
                paypalOrderId, approveUrl);

            // Store PayPal order ID in the database order
            Order.TrxId = paypalOrderId;
            _orderService.Update(Order);

            // Redirect user to PayPal (they will approve/cancel there)
            return Redirect(approveUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayPal order");
            ModelState.AddModelError("", "Failed to initiate PayPal payment. Please try again.");
            return Page();
        }
    }
}
```

### 4. Create PayPal Return Page

Create `Pages/Store/PayPalReturn.cshtml.cs`:

```csharp
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
        private readonly ILogger<PayPalReturnModel> _logger;

        public PayPalReturnModel(
            IShoppingCartService cartService,
            IProductService productService,
            IOrderService orderService,
            IOrderDetailService orderDetailService,
            IPayPalRedirectService payPalRedirectService,
            ILogger<PayPalReturnModel> logger)
        {
            _cartService = cartService;
            _productService = productService;
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _payPalRedirectService = payPalRedirectService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync([FromQuery] string token)
        {
            // PayPal returns with ?token={paypal_order_id}
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("PayPal return called without token");
                TempData["Error"] = "Invalid PayPal return. Please try again.";
                return RedirectToPage("/Store/Index");
            }

            var userId = User.FindFirst("sub")?.Value ?? 
                        User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("PayPal return: user not authenticated");
                return RedirectToPage("/Identity/Account/Login");
            }

            try
            {
                // Find pending order by PayPal order ID (stored in TrxId)
                var order = _orderService.GetAll()
                    .Where(o => o.TrxId == token && 
                               o.UserId == userId && 
                               o.TrxResponseCode == "PENDING")
                    .FirstOrDefault();

                if (order == null)
                {
                    _logger.LogWarning("PayPal return: no pending order found for token {Token} and user {UserId}", 
                        token, userId);
                    TempData["Error"] = "Order not found or already processed.";
                    return RedirectToPage("/Store/Index");
                }

                _logger.LogInformation("Capturing PayPal order {PayPalOrderId} for order {OrderId}", 
                    token, order.Id);

                // Build minimal request for capture
                var request = new PaymentRequest
                {
                    TransactionId = order.Id,
                    Amount = order.Total,
                    CurrencyCode = "USD",
                    InvoiceNumber = order.InvoiceNumber,
                    Description = "Portal Store Purchase"
                };

                // Capture the approved PayPal payment
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

                    TempData["Error"] = $"Payment failed: {paymentResponse.ErrorMessage}";
                    return RedirectToPage("/Store/Checkout");
                }

                // Update order with success
                order.TrxApproved = true;
                order.TrxId = paymentResponse.TransactionId; // Actual capture transaction ID
                order.TrxAuthorizationCode = paymentResponse.AuthorizationCode;
                order.TrxMessage = paymentResponse.Message;
                order.TrxResponseCode = paymentResponse.ResponseCode;
                _orderService.Update(order);

                // Get cart items and create order details
                var cartItems = _cartService.GetAll()
                    .Where(c => c.UserId == userId)
                    .ToList();

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
                TempData["Error"] = "An error occurred processing your payment. Please contact support.";
                return RedirectToPage("/Store/Index");
            }
        }
    }
}
```

Create `Pages/Store/PayPalReturn.cshtml`:

```html
@page
@model PayPalReturnModel
@{
    ViewData["Title"] = "Processing Payment";
}

<div class="container mt-5">
    <div class="row justify-content-center">
        <div class="col-md-8 text-center">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <h3 class="mt-3">Processing your PayPal payment...</h3>
            <p class="text-muted">Please wait while we complete your order.</p>
        </div>
    </div>
</div>
```

## Payment Flow Diagram

```
???????????????
?   Browser   ?
???????????????
       ? 1. POST /Checkout (with form data)
       ?
??????????????????????????????????
?  Checkout.cshtml.cs            ?
?  - Save pending order to DB    ?
?  - Create PayPal order         ?
?  - Get approval URL            ?
?  - Store PayPal order ID in DB ?
??????????????????????????????????
              ? 2. Redirect to PayPal
              ?
       ????????????????
       ?    PayPal    ?
       ?  (External)  ?
       ?              ?
       ? User approves?
       ?  payment     ?
       ????????????????
              ? 3. Redirect back with ?token=ORDER_ID
              ?
????????????????????????????????????
?  PayPalReturn.cshtml.cs          ?
?  - Find pending order by token   ?
?  - Capture approved payment      ?
?  - Update order with result      ?
?  - Create order details          ?
?  - Clear shopping cart           ?
????????????????????????????????????
               ? 4. Redirect to confirmation
               ?
       ??????????????????
       ?  Confirmation  ?
       ?      Page      ?
       ??????????????????
```

## Return URLs

PayPal needs return URLs configured in the order creation. The library automatically generates these based on your request:

```csharp
var returnBaseUrl = $"{Request.Scheme}://{Request.Host}";
// Development: https://localhost:7215
// Production:  https://yourdomain.com

// PayPal will use:
// Success: {returnBaseUrl}/Store/PayPalReturn?token={order_id}
// Cancel:  {returnBaseUrl}/Store/Checkout
```

**No hardcoding required!** The URLs adapt automatically to your environment.

## Testing

### Sandbox Testing

1. **Create Sandbox Accounts**:
   - Go to https://developer.paypal.com
   - Navigate to **Sandbox** ? **Accounts**
   - Create a **Business** account (seller)
   - Create a **Personal** account (buyer)

2. **Get Sandbox Credentials**:
   - Go to **My Apps & Credentials**
   - Switch to **Sandbox**
   - Create or select an app
   - Copy **Client ID** and **Secret**

3. **Test Flow**:
   - Set `IsTestMode: true` in appsettings
   - Add items to cart
   - Click checkout
   - You'll be redirected to PayPal sandbox
   - Log in with your **sandbox personal account**
   - Approve the payment
   - You'll be redirected back to your site
   - Order should complete successfully

### Test Checklist

- [ ] Order created in database with `TrxResponseCode = "PENDING"`
- [ ] Redirect to PayPal works
- [ ] Can log in to PayPal sandbox
- [ ] Can approve payment on PayPal
- [ ] Redirect back to `/Store/PayPalReturn?token=...` works
- [ ] Order found in database by token
- [ ] Payment captured successfully
- [ ] Order updated with `TrxApproved = true`
- [ ] Order details created
- [ ] Shopping cart cleared
- [ ] Redirect to confirmation page works

## Common Issues

### Issue: "ORDER_NOT_APPROVED"

**Cause**: Trying to capture before user approves  
**Solution**: Use the redirect flow (don't call `ProcessPaymentAsync` directly for PayPal)

### Issue: "Order not found" on return

**Cause**: Order not in database or query doesn't match  
**Solution**: 
- Verify order saved with `TrxResponseCode = "PENDING"`
- Check `TrxId` equals the `token` parameter
- Verify `userId` matches

### Issue: TempData lost

**Cause**: External redirect to PayPal clears TempData  
**Solution**: Save order to database before redirect (don't rely on TempData)

### Issue: Return URL doesn't match

**Cause**: PayPal strict about return URL format  
**Solution**: Use the provided pattern: `$"{Request.Scheme}://{Request.Host}/Store/PayPalReturn"`

## Security Considerations

1. **Validate User**: Always verify `userId` matches order owner on return
2. **Check Order Status**: Only capture orders with `TrxResponseCode = "PENDING"`
3. **Use HTTPS**: Required for production PayPal integration
4. **Log Everything**: Track order creation, redirects, and captures
5. **Handle Cancellations**: User can cancel on PayPal (redirect to cancel URL)
6. **Clean Up**: Consider background job to clean up abandoned orders

## Production Deployment

1. **Update Configuration**:
   ```json
   {
     "PaymentProviders": {
       "PayPal": {
         "ClientId": "YOUR_PRODUCTION_CLIENT_ID",
         "ClientSecret": "YOUR_PRODUCTION_CLIENT_SECRET",
         "IsTestMode": false
       }
     }
   }
   ```

2. **Verify HTTPS**: Production must use HTTPS

3. **Test Return URLs**: Verify return URLs resolve correctly in production

4. **Monitor Logs**: Watch for errors on first production transactions

5. **Set Up Alerts**: Monitor for failed captures or abandoned orders

## API Reference

### PayPalProvider Methods

```csharp
// Create order and get approval URL (Step 1)
Task<(string orderId, string approveUrl)> CreateOrderAndGetApprovalUrlAsync(
    PaymentRequest request, 
    string returnBaseUrl)

// Capture approved order (Step 2)
Task<PaymentResponse> CaptureApprovedOrderAsync(
    string orderId, 
    PaymentRequest originalRequest)

// Refund a captured payment
Task<PaymentResponse> RefundAsync(
    string transactionId, 
    decimal? amount = null)
```

### IPayPalRedirectService Methods

```csharp
// Wrapper for CreateOrderAndGetApprovalUrlAsync
Task<(string orderId, string approveUrl)> CreateOrderAsync(
    PaymentRequest request, 
    string returnBaseUrl)

// Wrapper for CaptureApprovedOrderAsync
Task<PaymentResponse> CaptureOrderAsync(
    string paypalOrderId, 
    PaymentRequest originalRequest)
```

## Additional Resources

- [PayPal REST API Documentation](https://developer.paypal.com/api/rest/)
- [PayPal Orders API v2](https://developer.paypal.com/docs/api/orders/v2/)
- [PayPal Sandbox Testing](https://developer.paypal.com/tools/sandbox/)
- [PayPal Integration Patterns](https://developer.paypal.com/docs/checkout/standard/integrate/)
