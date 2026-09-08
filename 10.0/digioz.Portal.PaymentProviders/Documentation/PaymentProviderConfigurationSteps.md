# Payment Provider Configuration - Administrator Guide

## Overview

The digioz Portal uses a single payment provider configured by the administrator. Users do not select a payment method during checkout; the system uses the provider configured by the admin.

The payment providers library supports two integration patterns:
- **Direct Card Processing** (Authorize.Net): Server-side, immediate charge
- **Redirect-Based Processing** (PayPal): User redirects to PayPal for approval

## Configuration Steps

### Step 1: Add Project Reference (Developer Task)

If not already done, add the PaymentProviders reference to `digioz.Portal.Web.csproj`:

```xml
<ProjectReference Include="..\digioz.Portal.PaymentProviders\digioz.Portal.PaymentProviders.csproj" />
```

### Step 2: Configure HttpClient for Payment Providers (Developer Task)

Add HttpClient configuration in `Program.cs` **before** registering payment providers:

```csharp
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
```

### Step 3: Register Payment Providers in Program.cs (Developer Task)

Add to `Program.cs` after HttpClient configuration:

```csharp
using digioz.Portal.PaymentProviders.DependencyInjection;
using digioz.Portal.Web.Services;

// Add payment providers configuration
builder.Services.AddPaymentProviders(paymentProviderBuilder =>
{
    var configuration = builder.Configuration;

    // Configure Authorize.Net
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

    // Configure PayPal (REST API: ClientId and ClientSecret)
    var paypalConfig = configuration.GetSection("PaymentProviders:PayPal");
    if (paypalConfig.Exists())
    {
        paymentProviderBuilder.ConfigureProvider("PayPal", config =>
        {
            config.ApiKey = paypalConfig["ClientId"];       // PayPal REST ClientId
            config.ApiSecret = paypalConfig["ClientSecret"]; // PayPal REST ClientSecret
            config.IsTestMode = paypalConfig.GetValue<bool>("IsTestMode");
        });
    }
});

// Register PayPal redirect service (required for PayPal integration)
builder.Services.AddScoped<IPayPalRedirectService, PayPalRedirectService>();
```

### Step 4: Configure API Credentials in appsettings.json (Developer Task)

Add to `appsettings.json`:

```json
{
  "PaymentProviders": {
    "AuthorizeNet": {
      "ApiKey": "YOUR_AUTHORIZE_NET_LOGIN_ID",
      "ApiSecret": "YOUR_AUTHORIZE_NET_TRANSACTION_KEY",
      "IsTestMode": false
    },
    "PayPal": {
      "ClientId": "YOUR_PAYPAL_REST_CLIENT_ID",
      "ClientSecret": "YOUR_PAYPAL_REST_CLIENT_SECRET",
      "IsTestMode": false
    }
  }
}
```

For development/sandbox, use `appsettings.Development.json`:

```json
{
  "PaymentProviders": {
    "AuthorizeNet": {
      "ApiKey": "YOUR_SANDBOX_LOGIN_ID",
      "ApiSecret": "YOUR_SANDBOX_TRANSACTION_KEY",
      "IsTestMode": true
    },
    "PayPal": {
      "ClientId": "YOUR_SANDBOX_PAYPAL_CLIENT_ID",
      "ClientSecret": "YOUR_SANDBOX_PAYPAL_CLIENT_SECRET",
      "IsTestMode": true
    }
  }
}
```

**Important Notes:**
- **Authorize.Net**: Uses `ApiKey` (Login ID) and `ApiSecret` (Transaction Key)
- **PayPal**: Uses REST API v2 with `ClientId` and `ClientSecret` (OAuth 2.0)
- Set `IsTestMode: true` for sandbox/testing environments
- Set `IsTestMode: false` for production environments

### Step 5: Create PayPal Return Handler (Developer Task - PayPal Only)

PayPal requires a return page to handle the redirect back from PayPal after approval.

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
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("PayPal return called without token");
                TempData["Error"] = "Invalid PayPal return.";
                return RedirectToPage("/Store/Index");
            }

            var userId = User.FindFirst("sub")?.Value ?? 
                        User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            // Find pending order by PayPal token
            var order = _orderService.GetAll()
                .Where(o => o.TrxId == token && 
                           o.UserId == userId && 
                           o.TrxResponseCode == "PENDING")
                .FirstOrDefault();

            if (order == null)
            {
                _logger.LogWarning("PayPal return: order not found for token {Token}", token);
                TempData["Error"] = "Order not found.";
                return RedirectToPage("/Store/Index");
            }

            // Capture the approved payment
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
                // Update order with success
                order.TrxApproved = true;
                order.TrxId = response.TransactionId;
                order.TrxAuthorizationCode = response.AuthorizationCode;
                order.TrxMessage = response.Message;
                order.TrxResponseCode = response.ResponseCode;
                _orderService.Update(order);

                // Create order details and clear cart
                var cartItems = _cartService.GetAll().Where(c => c.UserId == userId).ToList();
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

### Step 6: Update Checkout Logic (Developer Task)

Update your checkout page model to handle both providers:

```csharp
public class CheckoutModel : PageModel
{
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IPayPalRedirectService _payPalRedirectService;
    private readonly IOrderService _orderService;
    private readonly IConfigService _configService;

    public async Task<IActionResult> OnPostAsync()
    {
        // Get configured payment provider
        var providerName = GetConfiguredPaymentProvider(); // From Config table

        if (string.Equals(providerName, "PayPal", StringComparison.OrdinalIgnoreCase))
        {
            // PayPal redirect flow
            // Save pending order to database
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
        else
        {
            // Authorize.Net direct card processing
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
                _orderService.Add(Order);
                return RedirectToPage("OrderConfirmation", new { orderId = Order.Id });
            }
            else
            {
                ModelState.AddModelError("", response.ErrorMessage);
                return Page();
            }
        }
    }

    private string GetConfiguredPaymentProvider()
    {
        var config = _configService.GetByKey("PaymentProvider");
        return config?.ConfigValue?.Trim() ?? string.Empty;
    }
}
```

### Step 7: Set Active Payment Provider (Administrator Task)

The administrator sets which payment provider to use via the configuration database.

**Option A: Using Admin Panel**
1. Log in to the application as administrator
2. Navigate to **Admin** ? **Configuration** (or **Settings**)
3. Find or create the configuration key: `PaymentProvider`
4. Set the value to: `AuthorizeNet` or `PayPal`
5. Save the configuration

**Option B: Using SQL**
```sql
-- Check if configuration already exists
SELECT * FROM Config WHERE ConfigKey = 'PaymentProvider'

-- Insert new configuration
INSERT INTO Config (Id, ConfigKey, ConfigValue, [Description])
VALUES (NEWID(), 'PaymentProvider', 'AuthorizeNet', 'The payment provider used for all transactions')

-- Or update existing configuration
UPDATE Config 
SET ConfigValue = 'PayPal'
WHERE ConfigKey = 'PaymentProvider'
```

## Provider Comparison

| Feature | Authorize.Net | PayPal |
|---------|---------------|--------|
| Integration Type | Direct (server-side) | Redirect (user approval) |
| Card Collection | Your site | PayPal site |
| User Flow | Single page checkout | Redirect ? Approve ? Return |
| API Type | AIM (Advanced Integration Method) | REST API v2 (Orders API) |
| Configuration | ApiKey, ApiSecret | ClientId, ClientSecret |
| Return URL | Not required | Required (`/Store/PayPalReturn`) |
| Payment Method | `ProcessPaymentAsync()` | `CreateOrderAsync()` + `CaptureOrderAsync()` |

## Changing the Payment Provider

To switch from one payment provider to another:

1. **Using Admin Panel**: Edit the `PaymentProvider` configuration value
2. **Using SQL**: Update the Config table with the new provider name
3. **Effective Immediately**: The change takes effect for all new transactions

### Supported Provider Names
- `AuthorizeNet` - Authorize.net payment gateway (direct card processing)
- `PayPal` - PayPal payment gateway (redirect-based approval)

## Verification

To verify the payment provider is configured correctly:

1. Go to Admin ? Configuration
2. Look for the `PaymentProvider` key
3. Verify the value is set to your chosen provider
4. Process a test transaction to confirm functionality

## Getting API Credentials

### Authorize.Net
1. Log in to your Authorize.Net merchant account
2. Navigate to **Account** ? **Settings** ? **API Credentials & Keys**
3. Copy your **API Login ID** (ApiKey)
4. Generate or copy your **Transaction Key** (ApiSecret)
5. For sandbox testing, use credentials from your sandbox account

### PayPal (REST API)
1. Go to https://developer.paypal.com
2. Log in with your PayPal account
3. Navigate to **Dashboard** ? **My Apps & Credentials**
4. Under **REST API apps**, create a new app or select existing
5. Copy **Client ID** and **Secret**
6. For sandbox testing:
   - Switch to **Sandbox** tab
   - Create or select a sandbox app
   - Use sandbox credentials in `appsettings.Development.json`

## Test Card Numbers

### Authorize.Net Sandbox
- **Approved**: 4111111111111111
- **Declined**: 4222222222222220
- **Expiration**: Any future date (MM/YY)
- **CVV**: Any 3 digits

### PayPal Sandbox
1. Create sandbox accounts at https://developer.paypal.com
2. Navigate to **Sandbox** ? **Accounts**
3. Create a **Personal** account (buyer) and **Business** account (seller)
4. Use sandbox personal account to test payments
5. Refer to [PayPal Sandbox Documentation](https://developer.paypal.com/docs/api-basics/sandbox/)

## Troubleshooting

### Payment Provider Not Configured Error
**Symptom**: "Payment provider is not configured"  
**Solution**: 
- Verify the `PaymentProvider` configuration key exists in the Config table
- Check that the ConfigValue is set to a valid provider name (AuthorizeNet or PayPal)
- Verify the application has restarted to load the configuration

### Payment Provider Not Available Error
**Symptom**: "Payment provider is not configured" (during payment)  
**Solution**:
- Verify the provider name in config matches a registered provider exactly (case-insensitive)
- Check that API credentials are set in appsettings.json
- Verify HttpClient is registered for the provider
- Check that `AddPaymentProviders()` is called in Program.cs
- Review application logs for more details

### PayPal: ORDER_NOT_APPROVED Error
**Symptom**: Payment fails with "ORDER_NOT_APPROVED"  
**Solution**:
- This occurs if trying to capture before user approves
- Ensure you're using the redirect flow (not calling `ProcessPaymentAsync` directly)
- Verify `PayPalReturn.cshtml.cs` exists and is properly configured
- Check that order is saved with `TrxResponseCode = "PENDING"` before redirect

### PayPal: Order Not Found on Return
**Symptom**: "Order not found" when returning from PayPal  
**Solution**:
- Verify order is saved to database before redirect
- Check that `TrxId` matches the `token` query parameter
- Verify `TrxResponseCode = "PENDING"`
- Check that `userId` matches between order creation and return

### Payment Processing Fails
**Symptom**: Payments are declined or failing  
**Solution**:
- Verify API credentials in configuration are correct
- Check the environment (test vs. production) matches your credentials
- Verify `IsTestMode` setting matches your account type
- For Authorize.Net: Check card details format and validity
- For PayPal: Check that return URL is accessible
- Review application logs for provider-specific error messages

### Return URL Issues (PayPal)
**Symptom**: PayPal doesn't redirect back properly  
**Solution**:
- Verify `/Store/PayPalReturn` page exists
- Check that the page is accessible (not blocked by authorization)
- Ensure HTTPS is used in production
- Verify no firewall/proxy blocking the return

## Configuration Checklist

### Developer Setup
- [ ] PaymentProviders project reference added to Web.csproj
- [ ] HttpClient configured for both providers in Program.cs
- [ ] `AddPaymentProviders()` registration added to Program.cs
- [ ] `IPayPalRedirectService` registered in Program.cs
- [ ] API credentials added to `appsettings.Development.json`
- [ ] PayPal return page created (`PayPalReturn.cshtml` and `.cs`)
- [ ] Checkout logic updated to branch by provider type
- [ ] Test payment processed successfully with both providers

### Administrator Setup
- [ ] Payment provider selected (Authorize.Net or PayPal)
- [ ] `PaymentProvider` configuration key created with appropriate value
- [ ] Configuration verified in admin panel
- [ ] Test payment processed successfully
- [ ] Production credentials configured in production environment

## Security Considerations

1. **HTTPS Required**: All payment pages must use HTTPS (especially in production)
2. **Credentials Storage**: 
   - Use Azure Key Vault, AWS Secrets Manager, or environment variables for production
   - Never commit credentials to source control
3. **PCI DSS Compliance**: Required for Authorize.Net (direct card processing)
4. **Validate Return URLs**: For PayPal, verify the return is from PayPal
5. **Log Carefully**: Never log card numbers, CVV codes, or full API credentials
6. **Clean Up Pending Orders**: Implement background job to clean up abandoned PayPal orders

## Production Deployment

### Pre-Deployment Checklist
- [ ] `IsTestMode: false` in production appsettings
- [ ] Production credentials configured
- [ ] HTTPS enabled on all payment pages
- [ ] Return URLs tested and accessible
- [ ] Error handling and logging implemented
- [ ] Monitoring and alerts configured

### PayPal-Specific Production Steps
1. Switch from sandbox to production credentials
2. Verify return URL is publicly accessible
3. Test the full redirect flow in production
4. Monitor first few transactions closely

## Summary

The payment provider is configured centrally by the administrator. The system supports two integration patterns:

- **Authorize.Net**: Users enter card details on your site; payment is processed server-side immediately
- **PayPal**: Users are redirected to PayPal to approve the payment, then redirected back to complete the order

To change providers, the administrator only needs to update one configuration value in the Config table. The checkout process automatically adapts to the selected provider.

## Additional Resources

- [Authorize.Net Developer Documentation](https://developer.authorize.net/)
- [PayPal REST API Documentation](https://developer.paypal.com/api/rest/)
- [PayPal Orders API v2](https://developer.paypal.com/docs/api/orders/v2/)
- [PCI DSS Compliance Guide](https://www.pcisecuritystandards.org/)
