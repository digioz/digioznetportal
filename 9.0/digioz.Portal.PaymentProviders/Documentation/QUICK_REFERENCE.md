# Quick Reference Guide - digioz.Portal.PaymentProviders

## Quick Start (5 minutes)

### 1. Add to appsettings.json
```json
{
  "PaymentProviders": {
    "AuthorizeNet": {
      "ApiKey": "YOUR_LOGIN",
      "ApiSecret": "YOUR_TRANSACTION_KEY",
      "IsTestMode": true
    },
    "PayPal": {
      "ClientId": "YOUR_CLIENT_ID",
      "ClientSecret": "YOUR_CLIENT_SECRET",
      "IsTestMode": true
    }
  }
}
```

### 2. Update Program.cs
```csharp
using digioz.Portal.PaymentProviders.DependencyInjection;
using digioz.Portal.Web.Services;

// Configure HttpClients
builder.Services.AddHttpClient<AuthorizeNetProvider>();
builder.Services.AddHttpClient<PayPalProvider>();

// Add payment providers
builder.Services.AddPaymentProviders(paymentProviderBuilder =>
{
    var config = builder.Configuration;
    
    // Authorize.Net
    var authNetConfig = config.GetSection("PaymentProviders:AuthorizeNet");
    if (authNetConfig.Exists())
    {
        paymentProviderBuilder.ConfigureProvider("AuthorizeNet", cfg =>
        {
            cfg.ApiKey = authNetConfig["ApiKey"];
            cfg.ApiSecret = authNetConfig["ApiSecret"];
            cfg.IsTestMode = authNetConfig.GetValue<bool>("IsTestMode");
        });
    }
    
    // PayPal
    var paypalConfig = config.GetSection("PaymentProviders:PayPal");
    if (paypalConfig.Exists())
    {
        paymentProviderBuilder.ConfigureProvider("PayPal", cfg =>
        {
            cfg.ApiKey = paypalConfig["ClientId"];
            cfg.ApiSecret = paypalConfig["ClientSecret"];
            cfg.IsTestMode = paypalConfig.GetValue<bool>("IsTestMode");
        });
    }
});

// PayPal redirect service
builder.Services.AddScoped<IPayPalRedirectService, PayPalRedirectService>();
```

## Provider Comparison

| Feature | Authorize.Net | PayPal |
|---------|---------------|--------|
| Integration Type | Direct (server-side) | Redirect (user approval) |
| Card Collection | Your site | PayPal site |
| User Flow | Single page | Redirect ? Approve ? Return |
| Configuration | ApiKey, ApiSecret | ClientId, ClientSecret |
| Return URL | N/A | Dynamic (auto-generated) |
| API | AIM | REST v2 (Orders) |

## Common Operations

### Direct Card Processing (Authorize.Net)

```csharp
public class CheckoutModel : PageModel
{
    private readonly IPaymentProviderFactory _factory;
    
    public async Task<IActionResult> OnPostAsync()
    {
        var provider = _factory.CreateProvider("AuthorizeNet");
        
        var request = new PaymentRequest
        {
            TransactionId = Order.Id,
            Amount = Order.Total,
            CardNumber = Order.Ccnumber,
            ExpirationMonth = "12",
            ExpirationYear = "2025",
            CardCode = "123",
            CardholderName = $"{Order.FirstName} {Order.LastName}",
            // ... other fields
        };
        
        var response = await provider.ProcessPaymentAsync(request);
        
        if (response.IsApproved)
        {
            // Success
            Order.TrxId = response.TransactionId;
            Order.TrxApproved = true;
            _orderService.Add(Order);
            return RedirectToPage("Confirmation");
        }
        else
        {
            // Failed
            ModelState.AddModelError("", response.ErrorMessage);
            return Page();
        }
    }
}
```

### Redirect Processing (PayPal)

#### Step 1: Checkout Page - Create Order & Redirect

```csharp
public class CheckoutModel : PageModel
{
    private readonly IPayPalRedirectService _payPalRedirectService;
    private readonly IOrderService _orderService;

    public async Task<IActionResult> OnPostAsync()
    {
        // Save pending order
        Order.TrxApproved = false;
        Order.TrxResponseCode = "PENDING";
        Order.TrxMessage = "Awaiting PayPal approval";
        _orderService.Add(Order);

        // Create PayPal order
        var request = new PaymentRequest
        {
            TransactionId = Order.Id,
            Amount = Order.Total,
            CurrencyCode = "USD",
            InvoiceNumber = Order.InvoiceNumber
        };

        var returnBaseUrl = $"{Request.Scheme}://{Request.Host}";
        var (paypalOrderId, approveUrl) = await _payPalRedirectService
            .CreateOrderAsync(request, returnBaseUrl);

        // Store PayPal order ID
        Order.TrxId = paypalOrderId;
        _orderService.Update(Order);

        // Redirect to PayPal
        return Redirect(approveUrl);
    }
}
```

#### Step 2: Return Page - Capture Payment

```csharp
[Authorize]
public class PayPalReturnModel : PageModel
{
    private readonly IPayPalRedirectService _payPalRedirectService;
    private readonly IOrderService _orderService;

    public async Task<IActionResult> OnGetAsync([FromQuery] string token)
    {
        var userId = User.FindFirst("sub")?.Value;

        // Find pending order
        var order = _orderService.GetAll()
            .Where(o => o.TrxId == token && 
                       o.UserId == userId && 
                       o.TrxResponseCode == "PENDING")
            .FirstOrDefault();

        if (order == null)
        {
            TempData["Error"] = "Order not found.";
            return RedirectToPage("/Store/Index");
        }

        // Capture payment
        var request = new PaymentRequest
        {
            TransactionId = order.Id,
            Amount = order.Total,
            CurrencyCode = "USD",
            InvoiceNumber = order.InvoiceNumber
        };

        var response = await _payPalRedirectService.CaptureOrderAsync(token, request);

        if (response.IsApproved)
        {
            order.TrxApproved = true;
            order.TrxId = response.TransactionId;
            order.TrxAuthorizationCode = response.AuthorizationCode;
            order.TrxMessage = response.Message;
            order.TrxResponseCode = response.ResponseCode;
            _orderService.Update(order);

            // Clear cart, create order details, etc.
            return RedirectToPage("OrderConfirmation", new { orderId = order.Id });
        }
        else
        {
            order.TrxMessage = response.ErrorMessage;
            order.TrxResponseCode = response.ResponseCode;
            _orderService.Update(order);

            TempData["Error"] = $"Payment failed: {response.ErrorMessage}";
            return RedirectToPage("/Store/Checkout");
        }
    }
}
```

### Refund a Payment

```csharp
var provider = _factory.CreateProvider("AuthorizeNet"); // or "PayPal"

// Full refund
var refund = await provider.RefundAsync("TRANSACTION_ID");

// Partial refund
var partialRefund = await provider.RefundAsync("TRANSACTION_ID", 50.00m);

if (refund.IsApproved)
{
    // Refund successful
}
```

### Check Available Providers

```csharp
var providers = _factory.GetAvailableProviders();
// Returns: ["AuthorizeNet", "PayPal"]

if (_factory.IsProviderAvailable("Stripe"))
{
    // Stripe is available
}
```

## Configuration Cheat Sheet

### appsettings.json Structure

```json
{
  "PaymentProviders": {
    "AuthorizeNet": {
      "ApiKey": "string (login)",
      "ApiSecret": "string (transaction key)",
      "IsTestMode": "bool"
    },
    "PayPal": {
      "ClientId": "string (OAuth client ID)",
      "ClientSecret": "string (OAuth client secret)",
      "IsTestMode": "bool"
    }
  }
}
```

### PaymentProviderConfig
```csharp
var config = new PaymentProviderConfig
{
    ApiKey = "...",        // Login/ClientId
    ApiSecret = "...",     // Key/ClientSecret
    IsTestMode = true,     // Use sandbox
    Options = new Dictionary<string, string>
    {
        { "custom_key", "custom_value" }
    }
};
```

## PaymentRequest Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| TransactionId | string | Yes | Unique order/transaction ID |
| Amount | decimal | Yes | Amount in dollars (e.g., 100.00) |
| CurrencyCode | string | No | Default: "USD" |
| CardNumber | string | Card* | Credit card number |
| ExpirationMonth | string | Card* | 2-digit month (e.g., "12") |
| ExpirationYear | string | Card* | 4-digit year (e.g., "2025") |
| CardCode | string | Card* | CVV code |
| CardholderName | string | No | Name on card |
| CustomerEmail | string | No | Customer email |
| CustomerPhone | string | No | Customer phone |
| BillingAddress | string | No | Billing street address |
| BillingCity | string | No | Billing city |
| BillingState | string | No | Billing state/province |
| BillingZip | string | No | Billing postal code |
| BillingCountry | string | No | Billing country |
| ShippingAddress | string | No | Shipping street address |
| ShippingCity | string | No | Shipping city |
| ShippingState | string | No | Shipping state/province |
| ShippingZip | string | No | Shipping postal code |
| ShippingCountry | string | No | Shipping country |
| InvoiceNumber | string | No | Invoice reference |
| Description | string | No | Transaction description |

*Card fields required for direct card processing (Authorize.Net), not required for PayPal redirect

## PaymentResponse Properties

| Property | Type | Meaning |
|----------|------|---------|
| IsApproved | bool | **True = Success, False = Declined/Error** |
| AuthorizationCode | string | Authorization code from gateway |
| TransactionId | string | Gateway's transaction ID (save this!) |
| Message | string | Human-readable message |
| ResponseCode | string | Gateway response code |
| Amount | decimal | Amount charged |
| ErrorMessage | string | Error description (if declined) |
| ErrorCode | string | Error code (if declined) |
| RawResponse | Dictionary | Full gateway response (JSON) |
| CustomData | Dictionary | Provider-specific data |

## Test Cards & Credentials

### Authorize.Net Sandbox
- **Approved**: 4111111111111111
- **Declined**: 4222222222222220
- **Expiration**: Any future date
- **CVV**: Any 3 digits

### PayPal Sandbox
1. Create sandbox account: https://developer.paypal.com
2. Get ClientId and ClientSecret from app dashboard
3. Use personal/business sandbox accounts for testing
4. Set `IsTestMode: true`

## Error Handling Pattern

```csharp
try
{
    var response = await provider.ProcessPaymentAsync(request);
    
    if (!response.IsApproved)
    {
        // Payment declined
        _logger.LogWarning("Payment declined: {ErrorCode} - {ErrorMessage}",
            response.ErrorCode, response.ErrorMessage);
        
        return new { success = false, message = response.ErrorMessage };
    }
    
    // Success
    return new { success = true, transactionId = response.TransactionId };
}
catch (ArgumentException ex)
{
    // Invalid request (validation failed)
    _logger.LogError(ex, "Invalid payment request");
    return new { success = false, message = "Invalid payment information" };
}
catch (Exception ex)
{
    // Unexpected error
    _logger.LogError(ex, "Payment processing error");
    return new { success = false, message = "Please try again later" };
}
```

## PayPal Return URLs

Return URLs are **automatically generated** based on the request:

```csharp
// In your checkout handler:
var returnBaseUrl = $"{Request.Scheme}://{Request.Host}";
// Development: "https://localhost:7215"
// Production: "https://yourdomain.com"

// PayPal will redirect to:
// - Success: {returnBaseUrl}/Store/PayPalReturn?token={order_id}
// - Cancel: {returnBaseUrl}/Store/Checkout
```

## Troubleshooting

### PayPal: "ORDER_NOT_APPROVED"
**Cause**: Trying to capture before user approves  
**Fix**: Use redirect flow - create order ? redirect ? capture on return

### PayPal Return: "Order not found"
**Cause**: Pending order not in database or TempData lost  
**Fix**: Save order with `TrxResponseCode = "PENDING"` before redirect

### Provider Not Found
```
ArgumentException: Payment provider 'PayPal' is not registered.
```
**Fix**: 
1. Check provider name (case-insensitive)
2. Verify `AddPaymentProviders()` called in `Program.cs`
3. Check configuration loaded from appsettings.json

### Invalid Credentials
```
Response: IsApproved = false, ErrorCode = "AUTHENTICATION_FAILURE"
```
**Fix**: 
1. Verify credentials in appsettings.json
2. Check `IsTestMode` matches account type (sandbox vs. production)
3. For PayPal: use ClientId/ClientSecret, not username/password

### Configuration is Null
```
NullReferenceException: Config is null
```
**Fix**: 
1. Ensure `PaymentProviders` section exists in appsettings.json
2. Call `ConfigureProvider()` before using provider

## Logging Best Practices

```csharp
// Log payment attempt (NO CARD DATA!)
_logger.LogInformation("Processing payment: {Provider}, Amount: {Amount}, OrderId: {OrderId}",
    providerName, request.Amount, request.TransactionId);

// Log response
_logger.LogInformation("Payment result: Provider={Provider}, Approved={IsApproved}, TxId={TransactionId}",
    providerName, response.IsApproved, response.TransactionId);

// Log declined payment
_logger.LogWarning("Payment declined: Provider={Provider}, ErrorCode={ErrorCode}, Message={ErrorMessage}",
    providerName, response.ErrorCode, response.ErrorMessage);

// NEVER LOG:
// - Card numbers
// - CVV codes
// - Full card details
```

## Service Registration Quick Reference

```csharp
// Basic setup
builder.Services.AddHttpClient<AuthorizeNetProvider>();
builder.Services.AddHttpClient<PayPalProvider>();
builder.Services.AddPaymentProviders();

// With configuration
builder.Services.AddPaymentProviders(b => {
    b.ConfigureProvider("AuthorizeNet", config =>
    {
        config.ApiKey = "...";
        config.ApiSecret = "...";
        config.IsTestMode = true;
    });
});

// PayPal redirect service (required for PayPal)
builder.Services.AddScoped<IPayPalRedirectService, PayPalRedirectService>();
```

## Production Checklist

- [ ] Set `IsTestMode = false` in production appsettings
- [ ] Use production credentials (not sandbox)
- [ ] HTTPS enabled on all pages
- [ ] Error handling implemented
- [ ] Logging configured (no sensitive data)
- [ ] PayPal return page created and tested
- [ ] Clean up pending orders (background job)
- [ ] Payment confirmation emails working
- [ ] Refund process tested
- [ ] Monitoring/alerts configured

## Documentation Files

- **README.md**: Full API documentation
- **INTEGRATION_GUIDE.md**: Step-by-step integration
- **BEST_PRACTICES.md**: Security and implementation guidelines
- **ARCHITECTURE.md**: Design patterns and structure
- **QUICK_REFERENCE.md**: This file
- **FILE_MANIFEST.md**: Complete file listing

## Key Interfaces

```csharp
// Create providers
IPaymentProviderFactory

// Process payments
IPaymentProvider
  - ProcessPaymentAsync(request)
  - RefundAsync(transactionId, amount?)
  - ValidateConfiguration()

// PayPal redirect flow
IPayPalRedirectService
  - CreateOrderAsync(request, returnBaseUrl)
  - CaptureOrderAsync(paypalOrderId, request)
```

## Need Help?

1. Check **INTEGRATION_GUIDE.md** for detailed steps
2. Review **README.md** for API documentation
3. Consult **BEST_PRACTICES.md** for security
4. Look at `Examples/` folder for sample code
5. Check provider documentation:
   - [Authorize.Net](https://developer.authorize.net/)
   - [PayPal REST API](https://developer.paypal.com/api/rest/)
