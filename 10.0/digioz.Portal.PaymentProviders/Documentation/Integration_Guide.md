# Integration Guide for digioz.Portal.PaymentProviders

This guide explains how to integrate the `digioz.Portal.PaymentProviders` library into the main digioz Portal web application.

## Overview

The payment providers library supports two integration patterns:

1. **Direct Card Processing** (Authorize.net) - Server-side, immediate charge
2. **Redirect-Based Processing** (PayPal) - User redirects to PayPal for approval

## Step 1: Update Project Reference

Add a reference to the payment providers library in `digioz.Portal.Web.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\digioz.Portal.PaymentProviders\digioz.Portal.PaymentProviders.csproj" />
</ItemGroup>
```

## Step 2: Configure in Program.cs

In your `Program.cs`, add the payment providers configuration:

```csharp
using digioz.Portal.PaymentProviders.DependencyInjection;
using digioz.Portal.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure HttpClient for payment providers BEFORE registering payment providers
builder.Services.AddHttpClient<digioz.Portal.PaymentProviders.Providers.AuthorizeNetProvider>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });

builder.Services.AddHttpClient<digioz.Portal.PaymentProviders.Providers.PayPalProvider>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });

// Add payment providers with configuration from appsettings.json
builder.Services.AddPaymentProviders(paymentProviderBuilder =>
{
    var configuration = builder.Configuration;

    var authNetConfig = configuration.GetSection("PaymentProviders:AuthorizeNet");
    if (authNetConfig.Exists())
    {
        paymentProviderBuilder.ConfigureProvider("AuthorizeNet", config =>
        {
            config.ApiKey = authNetConfig["ApiKey"];
            config.ApiSecret = authNetConfig["ApiSecret"];
            config.IsTestMode = authNetConfig.GetValue<bool>("IsTestMode");
        });
    }

    var paypalConfig = configuration.GetSection("PaymentProviders:PayPal");
    if (paypalConfig.Exists())
    {
        paymentProviderBuilder.ConfigureProvider("PayPal", config =>
        {
            // REST API: ClientId and ClientSecret
            config.ApiKey = paypalConfig["ClientId"];
            config.ApiSecret = paypalConfig["ClientSecret"];
            config.IsTestMode = paypalConfig.GetValue<bool>("IsTestMode");
        });
    }
});

// Register PayPal redirect service for redirect-based checkout
builder.Services.AddScoped<IPayPalRedirectService, PayPalRedirectService>();
```

## Step 3: Configure appsettings.json

Add payment provider configuration to your `appsettings.json`:

```json
{
  "PaymentProviders": {
    "AuthorizeNet": {
      "ApiKey": "YOUR_AUTHORIZE_NET_LOGIN",
      "ApiSecret": "YOUR_AUTHORIZE_NET_TRANSACTION_KEY",
      "IsTestMode": true
    },
    "PayPal": {
      "ClientId": "YOUR_PAYPAL_CLIENT_ID",
      "ClientSecret": "YOUR_PAYPAL_CLIENT_SECRET",
      "IsTestMode": true
    }
  }
}
```

For production environments, use `appsettings.Production.json`:

```json
{
  "PaymentProviders": {
    "AuthorizeNet": {
      "ApiKey": "YOUR_PRODUCTION_LOGIN",
      "ApiSecret": "YOUR_PRODUCTION_TRANSACTION_KEY",
      "IsTestMode": false
    },
    "PayPal": {
      "ClientId": "YOUR_PRODUCTION_CLIENT_ID",
      "ClientSecret": "YOUR_PRODUCTION_CLIENT_SECRET",
      "IsTestMode": false
    }
  }
}
```

## Step 4: Implement Checkout Logic (Razor Pages)

### Checkout Page Model (Checkout.cshtml.cs)

```csharp
using digioz.Portal.PaymentProviders.Abstractions;
using digioz.Portal.PaymentProviders.Models;
using digioz.Portal.Web.Services;

public class CheckoutModel : PageModel
{
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IPayPalRedirectService _payPalRedirectService;
    private readonly IOrderService _orderService;

    [BindProperty]
    public Order Order { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // Get configured payment provider
        var providerName = GetConfiguredPaymentProvider(); // e.g., "AuthorizeNet" or "PayPal"

        // Branch based on provider type
        if (string.Equals(providerName, "PayPal", StringComparison.OrdinalIgnoreCase))
        {
            return await ProcessPayPalRedirectAsync();
        }
        else
        {
            return await ProcessDirectCardPaymentAsync(providerName);
        }
    }

    private async Task<IActionResult> ProcessPayPalRedirectAsync()
    {
        // Save pending order to database
        Order.TrxApproved = false;
        Order.TrxResponseCode = "PENDING";
        Order.TrxMessage = "Awaiting PayPal approval";
        _orderService.Add(Order);

        // Build payment request
        var request = new PaymentRequest
        {
            TransactionId = Order.Id,
            Amount = Order.Total,
            CurrencyCode = "USD",
            InvoiceNumber = Order.InvoiceNumber,
            Description = "Store Purchase"
        };

        // Get base URL for PayPal callbacks
        var returnBaseUrl = $"{Request.Scheme}://{Request.Host}";

        // Create PayPal order and get approval URL
        var (paypalOrderId, approveUrl) = await _payPalRedirectService.CreateOrderAsync(request, returnBaseUrl);

        // Store PayPal order ID in order record
        Order.TrxId = paypalOrderId;
        _orderService.Update(Order);

        // Redirect user to PayPal
        return Redirect(approveUrl);
    }

    private async Task<IActionResult> ProcessDirectCardPaymentAsync(string providerName)
    {
        var provider = _paymentProviderFactory.CreateProvider(providerName);

        var request = new PaymentRequest
        {
            TransactionId = Order.Id,
            Amount = Order.Total,
            CurrencyCode = "USD",
            CardNumber = Order.Ccnumber,
            ExpirationMonth = Order.CcExpMonth,
            ExpirationYear = Order.CcExpYear,
            CardCode = Order.CccardCode,
            // ... other fields
        };

        var response = await provider.ProcessPaymentAsync(request);

        if (response.IsApproved)
        {
            Order.TrxApproved = true;
            Order.TrxId = response.TransactionId;
            Order.TrxAuthorizationCode = response.AuthorizationCode;
            _orderService.Add(Order);

            return RedirectToPage("OrderConfirmation", new { orderId = Order.Id });
        }
        else
        {
            ModelState.AddModelError("", $"Payment declined: {response.ErrorMessage}");
            return Page();
        }
    }
}
```

## Step 5: Create PayPal Return Handler

Create `PayPalReturn.cshtml.cs` to handle the redirect back from PayPal:

```csharp
[Authorize]
public class PayPalReturnModel : PageModel
{
    private readonly IPayPalRedirectService _payPalRedirectService;
    private readonly IOrderService _orderService;
    private readonly IShoppingCartService _cartService;

    public async Task<IActionResult> OnGetAsync([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["Error"] = "Invalid PayPal return.";
            return RedirectToPage("/Store/Index");
        }

        var userId = User.FindFirst("sub")?.Value;

        // Find pending order by PayPal order ID
        var order = _orderService.GetAll()
            .Where(o => o.TrxId == token && o.UserId == userId && o.TrxResponseCode == "PENDING")
            .FirstOrDefault();

        if (order == null)
        {
            TempData["Error"] = "Order not found.";
            return RedirectToPage("/Store/Index");
        }

        // Build request for capture
        var request = new PaymentRequest
        {
            TransactionId = order.Id,
            Amount = order.Total,
            CurrencyCode = "USD",
            InvoiceNumber = order.InvoiceNumber
        };

        // Capture the PayPal payment
        var response = await _payPalRedirectService.CaptureOrderAsync(token, request);

        if (response.IsApproved)
        {
            // Update order with success
            order.TrxApproved = true;
            order.TrxId = response.TransactionId;
            order.TrxAuthorizationCode = response.AuthorizationCode;
            order.TrxMessage = response.Message;
            order.TrxResponseCode = response.ResponseCode;
            _orderService.Update(order);

            // Clear cart, create order details, etc.
            // ...

            return RedirectToPage("OrderConfirmation", new { orderId = order.Id });
        }
        else
        {
            // Update order with failure
            order.TrxMessage = response.ErrorMessage;
            order.TrxResponseCode = response.ResponseCode;
            _orderService.Update(order);

            TempData["Error"] = $"Payment failed: {response.ErrorMessage}";
            return RedirectToPage("/Store/Checkout");
        }
    }
}
```

Create `PayPalReturn.cshtml`:

```html
@page
@model PayPalReturnModel
@{
    ViewData["Title"] = "Processing Payment";
}

<div class="container mt-5">
    <div class="text-center">
        <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
        </div>
        <h3 class="mt-3">Processing your PayPal payment...</h3>
        <p class="text-muted">Please wait.</p>
    </div>
</div>
```

## Step 6: Create PayPal Redirect Service

Create `IPayPalRedirectService.cs`:

```csharp
public interface IPayPalRedirectService
{
    Task<(string orderId, string approveUrl)> CreateOrderAsync(PaymentRequest request, string returnBaseUrl);
    Task<PaymentResponse> CaptureOrderAsync(string paypalOrderId, PaymentRequest originalRequest);
}

public class PayPalRedirectService : IPayPalRedirectService
{
    private readonly IPaymentProviderFactory _factory;

    public PayPalRedirectService(IPaymentProviderFactory factory)
    {
        _factory = factory;
    }

    public async Task<(string orderId, string approveUrl)> CreateOrderAsync(PaymentRequest request, string returnBaseUrl)
    {
        var provider = _factory.CreateProvider("PayPal") as PayPalProvider;
        if (provider == null)
            throw new InvalidOperationException("PayPal provider not available.");

        return await provider.CreateOrderAndGetApprovalUrlAsync(request, returnBaseUrl);
    }

    public async Task<PaymentResponse> CaptureOrderAsync(string paypalOrderId, PaymentRequest originalRequest)
    {
        var provider = _factory.CreateProvider("PayPal") as PayPalProvider;
        if (provider == null)
            throw new InvalidOperationException("PayPal provider not available.");

        return await provider.CaptureApprovedOrderAsync(paypalOrderId, originalRequest);
    }
}
```

## Payment Provider Comparison

| Feature | Authorize.Net | PayPal (REST) |
|---------|---------------|---------------|
| Integration Type | Direct (Server-side) | Redirect (User approval) |
| Card Data | Collected on your site | Not collected (PayPal handles) |
| User Experience | Single page checkout | Redirect to PayPal, then back |
| Payment Method | `ProcessPaymentAsync()` | `CreateOrderAndGetApprovalUrlAsync()` + `CaptureApprovedOrderAsync()` |
| Return URL | N/A | Dynamic (based on request host) |
| Configuration | ApiKey, ApiSecret | ClientId, ClientSecret |

## Testing

### Authorize.net Test Cards
- **Approved**: 4111111111111111
- **Declined**: 4222222222222220

### PayPal Sandbox
1. Create sandbox accounts at https://developer.paypal.com
2. Use sandbox ClientId and ClientSecret
3. Set `IsTestMode: true`
4. Test the full redirect flow

## Security Considerations

1. **Never log credit card numbers or CVV codes**
2. **Use HTTPS** - All payment communication must be encrypted
3. **Store credentials securely** - Use Azure Key Vault or similar
4. **Implement PCI DSS compliance** for card-based processing
5. **Validate return URLs** - Ensure PayPal returns match your domain
6. **Handle pending orders** - Clean up abandoned PayPal orders

## Error Handling

```csharp
try
{
    var response = await provider.ProcessPaymentAsync(request);
    
    if (!response.IsApproved)
    {
        // Handle declined payment
        _logger.LogWarning("Payment declined: {ErrorCode}", response.ErrorCode);
        ModelState.AddModelError("", $"Payment declined: {response.ErrorMessage}");
    }
}
catch (ArgumentException ex)
{
    // Invalid configuration or request
    _logger.LogError(ex, "Invalid payment request");
}
catch (Exception ex)
{
    // Unexpected error
    _logger.LogError(ex, "Payment processing error");
}
```

## Troubleshooting

### PayPal Return URL Issues
- Verify return URL format: `{scheme}://{host}/Store/PayPalReturn`
- Check logs for "PayPal return: no pending order found"
- Ensure order is saved with `TrxResponseCode = "PENDING"`
- Verify `token` query parameter is present

### Provider Not Found
- Ensure provider name matches registration (case-insensitive)
- Check that configuration is properly loaded
- Verify HttpClient is registered for the provider

### Configuration Issues
- Verify credentials in appsettings.json
- Check that `IsTestMode` matches your account type
- Use environment-specific configuration files

## Additional Resources

- [Authorize.net Documentation](https://developer.authorize.net/)
- [PayPal REST API Documentation](https://developer.paypal.com/api/rest/)
- [PayPal Orders API](https://developer.paypal.com/docs/api/orders/v2/)
- [PCI DSS Compliance](https://www.pcisecuritystandards.org/)

