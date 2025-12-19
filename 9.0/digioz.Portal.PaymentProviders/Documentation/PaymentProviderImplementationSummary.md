# Payment Provider Integration Implementation - Complete Summary

## Overview

The digioz Portal web application integrates a **single, administrator-configured payment provider** using the `digioz.Portal.PaymentProviders` library. The administrator determines which payment provider (Authorize.Net or PayPal) processes all transactions, and users do not select a payment method during checkout.

The library supports two integration patterns:
- **Direct Card Processing** (Authorize.Net): Server-side, immediate charge with card details collected on your site
- **Redirect-Based Processing** (PayPal): User redirects to PayPal for approval, then returns to complete order

## Key Design Decision

**Single Provider Model**: The administrator configures one payment provider that is used for all transactions. Users cannot select between providers during checkout - payment processing happens transparently using the configured provider.

## How Payment Provider Selection Works

### Decision Flow

```
Checkout form submitted
         ?
Application retrieves configured provider from database
         ?
Check provider type (Authorize.Net or PayPal)
         ?
         ??? Authorize.Net: Direct card processing
         ?         ?
         ?   Build payment request with card data
         ?         ?
         ?   Process payment immediately
         ?         ?
         ?   Save order with transaction details
         ?
         ??? PayPal: Redirect-based approval
                   ?
             Save pending order to database
                   ?
             Create PayPal order via REST API
                   ?
             Redirect user to PayPal
                   ?
             User approves payment on PayPal
                   ?
             PayPal redirects back with token
                   ?
             Find pending order by token
                   ?
             Capture approved payment
                   ?
             Update order and complete checkout
```

## Files Modified/Created

### 1. **digioz.Portal.PaymentProviders/Providers/PayPalProvider.cs**

**Major Changes**: Updated to use REST API v2 (Orders API) with redirect flow

**Key Methods**:

```csharp
// Create PayPal order and get approval URL
public async Task<(string orderId, string approveUrl)> CreateOrderAndGetApprovalUrlAsync(
    PaymentRequest request, 
    string returnBaseUrl)

// Capture approved PayPal payment
public async Task<PaymentResponse> CaptureApprovedOrderAsync(
    string orderId, 
    PaymentRequest originalRequest)

// Refund a captured payment
public override async Task<PaymentResponse> RefundAsync(
    string transactionId, 
    decimal? amount = null)
```

**Configuration**:
- Uses `ApiKey` = ClientId (OAuth 2.0)
- Uses `ApiSecret` = ClientSecret (OAuth 2.0)
- Supports sandbox and production environments
- Return URLs dynamically generated based on request host

### 2. **digioz.Portal.Web/Services/PayPalRedirectService.cs** (NEW)

**Purpose**: Encapsulates PayPal redirect flow for Razor Pages

**Interface**:
```csharp
public interface IPayPalRedirectService
{
    Task<(string orderId, string approveUrl)> CreateOrderAsync(
        PaymentRequest request, 
        string returnBaseUrl);
        
    Task<PaymentResponse> CaptureOrderAsync(
        string paypalOrderId, 
        PaymentRequest originalRequest);
}
```

**Registration**: `builder.Services.AddScoped<IPayPalRedirectService, PayPalRedirectService>();`

### 3. **digioz.Portal.Web/Pages/Store/Checkout.cshtml.cs**

**Major Changes**: Branching logic for Authorize.Net vs. PayPal

**Removed**:
- User selection of payment provider
- PaymentGateway property from view model

**Added**:
- `ProcessPayPalRedirectAsync()` method for PayPal flow
- Database persistence before redirect
- Dynamic return URL generation

**Key Code**:
```csharp
private async Task<IActionResult> ProcessPayPalRedirectAsync(string userId)
{
    // Save pending order to database BEFORE redirect
    Order.TrxApproved = false;
    Order.TrxResponseCode = "PENDING";
    Order.TrxMessage = "Awaiting PayPal approval";
    _orderService.Add(Order);

    // Create PayPal order
    var request = BuildPaymentRequest();
    var returnBaseUrl = $"{Request.Scheme}://{Request.Host}";
    var (paypalOrderId, approveUrl) = await _payPalRedirectService
        .CreateOrderAsync(request, returnBaseUrl);

    // Store PayPal order ID
    Order.TrxId = paypalOrderId;
    _orderService.Update(Order);

    // Redirect to PayPal
    return Redirect(approveUrl);
}
```

### 4. **digioz.Portal.Web/Pages/Store/PayPalReturn.cshtml.cs** (NEW)

**Purpose**: Handle redirect back from PayPal and capture payment

**Key Logic**:
```csharp
public async Task<IActionResult> OnGetAsync([FromQuery] string token)
{
    // Find pending order by PayPal token
    var order = _orderService.GetAll()
        .Where(o => o.TrxId == token && 
                   o.UserId == userId && 
                   o.TrxResponseCode == "PENDING")
        .FirstOrDefault();

    // Capture the approved payment
    var response = await _payPalRedirectService.CaptureOrderAsync(token, request);

    if (response.IsApproved)
    {
        // Update order, create order details, clear cart
        order.TrxApproved = true;
        order.TrxId = response.TransactionId;
        _orderService.Update(order);
        
        return RedirectToPage("OrderConfirmation", new { orderId = order.Id });
    }
}
```

### 5. **digioz.Portal.Web/Program.cs**

**Added**:
```csharp
// Configure HttpClient for providers
builder.Services.AddHttpClient<AuthorizeNetProvider>();
builder.Services.AddHttpClient<PayPalProvider>();

// Add payment providers
builder.Services.AddPaymentProviders(paymentProviderBuilder =>
{
    // Authorize.Net configuration
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

    // PayPal configuration (REST API)
    var paypalConfig = configuration.GetSection("PaymentProviders:PayPal");
    if (paypalConfig.Exists())
    {
        paymentProviderBuilder.ConfigureProvider("PayPal", config =>
        {
            config.ApiKey = paypalConfig["ClientId"];
            config.ApiSecret = paypalConfig["ClientSecret"];
            config.IsTestMode = paypalConfig.GetValue<bool>("IsTestMode");
        });
    }
});

// Register PayPal redirect service
builder.Services.AddScoped<IPayPalRedirectService, PayPalRedirectService>();
```

### 6. **digioz.Portal.Bo/ViewModels/CheckOutViewModel.cs**

**Removed**: `PaymentGateway` property (no longer needed)

## Architecture

### Component Relationships

```
Configuration Database (PaymentProvider = "AuthorizeNet" | "PayPal")
                ?
        Checkout Page Handler
                ?
    GetConfiguredPaymentProvider()
                ?
        ?????????????????
        ?               ?
  AuthorizeNet      PayPal
  (Direct)      (Redirect Flow)
        ?               ?
  ProcessPayment   CreateOrder ? Redirect ? Capture
```

### PayPal Redirect Flow Detail

```
???????????????
?   Browser   ?
???????????????
       ? 1. POST /Checkout
       ?
??????????????????????????????????
?  Checkout.cshtml.cs            ?
?  - Save pending order to DB    ?
?  - Create PayPal order         ?
?  - Get approval URL            ?
?  - Store PayPal order ID       ?
??????????????????????????????????
              ? 2. Redirect to PayPal
              ?
       ????????????????
       ?    PayPal    ?
       ?  (External)  ?
       ? User approves?
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
       ??????????????????
```

## Configuration Requirements

### 1. appsettings.json (Developer Task)

```json
{
  "PaymentProviders": {
    "AuthorizeNet": {
      "ApiKey": "YOUR_LOGIN_ID",
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

**Production** (`appsettings.Production.json`):
```json
{
  "PaymentProviders": {
    "AuthorizeNet": {
      "ApiKey": "YOUR_PRODUCTION_LOGIN_ID",
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

### 2. Set Active Provider (Administrator Task)

**Via Admin Panel**:
1. Navigate to Admin ? Configuration
2. Create/Edit key: `PaymentProvider`
3. Set value to: `AuthorizeNet` or `PayPal`
4. Save

**Via SQL**:
```sql
UPDATE Config 
SET ConfigValue = 'PayPal'  -- or 'AuthorizeNet'
WHERE ConfigKey = 'PaymentProvider'
```

## Provider Comparison

| Feature | Authorize.Net | PayPal |
|---------|---------------|--------|
| Integration Type | Direct (server-side) | Redirect (user approval) |
| Card Collection | Your site | PayPal site |
| User Experience | Single page checkout | Redirect ? Approve ? Return |
| API Type | AIM (Advanced Integration) | REST API v2 (Orders) |
| Configuration | ApiKey, ApiSecret | ClientId, ClientSecret |
| Return Page | Not required | Required (`/Store/PayPalReturn`) |
| OAuth | No | Yes (OAuth 2.0) |
| PCI DSS Compliance | Required | Not required (PayPal handles cards) |
| Payment Method | `ProcessPaymentAsync()` | `CreateOrderAsync()` + `CaptureOrderAsync()` |

## Features Implemented

? **Administrator Control**
- Admin determines which provider is used via configuration
- Change provider without code changes or deployment

? **Dual Integration Patterns**
- Direct card processing for Authorize.Net
- Redirect-based approval for PayPal

? **Dynamic Return URLs**
- PayPal return URLs automatically adapt to environment
- Works in development (localhost) and production

? **Database Persistence**
- Pending orders saved before PayPal redirect
- Survives external redirect (TempData would be lost)

? **Comprehensive Error Handling**
- Clear errors if provider not configured
- Detailed error information from providers
- Graceful fallback on payment failure
- PayPal-specific error handling (ORDER_NOT_APPROVED, etc.)

? **Audit Logging**
- All payment attempts logged with provider name
- Success/failure captured
- Error codes and messages recorded
- PayPal order creation and capture logged separately

? **Order Integration**
- Transaction details saved to Order entity
- Transaction ID, authorization code, response stored
- Pending order status for PayPal
- Enables order tracking and refunds

? **Security**
- HTTPS required for production
- OAuth 2.0 for PayPal
- User validation on PayPal return
- Order status validation before capture

## Testing

### Authorize.Net Testing
**Test Cards**:
- Approved: 4111111111111111
- Declined: 4222222222222220
- Expiration: Any future date (MM/YY)
- CVV: Any 3 digits

**Flow**:
1. Set `PaymentProvider = "AuthorizeNet"`
2. Add items to cart
3. Go to checkout
4. Enter test card details
5. Submit order
6. Verify payment processed immediately

### PayPal Testing
**Setup**:
1. Create sandbox accounts at https://developer.paypal.com
2. Get sandbox ClientId and ClientSecret
3. Set `IsTestMode: true` in appsettings

**Flow**:
1. Set `PaymentProvider = "PayPal"`
2. Add items to cart
3. Go to checkout
4. Fill in customer information (card not required)
5. Click submit
6. Verify redirect to PayPal sandbox
7. Log in with sandbox personal account
8. Approve payment
9. Verify redirect back to `/Store/PayPalReturn?token=...`
10. Verify order completed successfully

**Test Checklist**:
- [ ] Order saved with `TrxResponseCode = "PENDING"` before redirect
- [ ] PayPal order ID stored in `TrxId`
- [ ] Redirect to PayPal works
- [ ] Can approve payment on PayPal
- [ ] Redirect back with token parameter
- [ ] Order found by token and user
- [ ] Payment captured successfully
- [ ] Order updated with `TrxApproved = true`
- [ ] Order details created
- [ ] Cart cleared
- [ ] Confirmation page displayed

## Error Scenarios Handled

| Scenario | Response |
|----------|----------|
| Provider not configured | Error: "Payment provider is not configured. Please contact support." |
| Provider not registered | Error: "Payment provider is not configured" |
| Invalid card details (Authorize.Net) | Error from provider (e.g., "Invalid card number") |
| Declined transaction | Error: "Payment failed: [provider error message]" |
| PayPal ORDER_NOT_APPROVED | Error: "An error occurred capturing the PayPal order" |
| PayPal return without token | Error: "Invalid PayPal return. Please try again." |
| PayPal order not found | Error: "Order not found or already processed." |
| Network/System error | Error: "An error occurred processing your payment" |

## Security Considerations

? **Implemented**:
- Provider name stored in secure configuration database
- API credentials stored in secure appsettings files
- HTTPS required for all payment communication
- Card data not logged in full
- Configuration validated on application startup
- PayPal OAuth 2.0 authentication
- User validation on PayPal return
- Order status validation before capture
- Dynamic return URLs prevent hardcoded endpoints

?? **Recommended**:
- Use Azure Key Vault or AWS Secrets Manager for credentials
- Implement PCI DSS compliance for Authorize.Net
- Use tokenization for recurring payments
- Add rate limiting to payment endpoints
- Monitor payment failures for fraud patterns
- Implement background job to clean up abandoned PayPal orders
- Set up alerts for capture failures

## Benefits of Single Provider Model

? **Simplified Administration**: One configuration value to manage  
? **Consistent User Experience**: Same provider for all users  
? **Reduced Complexity**: No need for provider routing logic in UI  
? **Easier Troubleshooting**: All transactions use same provider  
? **Better Audit Trail**: Unified logging per provider  
? **Flexible**: Admin can switch providers via configuration  
? **Pattern Support**: Handles both direct and redirect patterns

## PayPal-Specific Implementation Details

### Return URL Generation
```csharp
var returnBaseUrl = $"{Request.Scheme}://{Request.Host}";
// Development: https://localhost:7215
// Production:  https://yourdomain.com

// PayPal uses:
// Success: {returnBaseUrl}/Store/PayPalReturn?token={order_id}
// Cancel:  {returnBaseUrl}/Store/Checkout
```

### Order Persistence Strategy
**Problem**: TempData is lost during external redirect to PayPal  
**Solution**: Save order to database with `TrxResponseCode = "PENDING"` before redirect

### Order Lookup on Return
```csharp
var order = _orderService.GetAll()
    .Where(o => o.TrxId == token &&           // PayPal order ID
               o.UserId == userId &&           // Security: verify owner
               o.TrxResponseCode == "PENDING") // Only pending orders
    .FirstOrDefault();
```

## Troubleshooting

### PayPal: "ORDER_NOT_APPROVED"
**Cause**: Trying to capture before user approves  
**Fix**: Use redirect flow (don't call `ProcessPaymentAsync` directly)

### PayPal: "Order not found" on return
**Cause**: Order not saved or query doesn't match  
**Fix**: Verify order saved with `TrxResponseCode = "PENDING"` and `TrxId` set

### PayPal: Return URL doesn't work
**Cause**: Page doesn't exist or authorization blocks it  
**Fix**: Verify `/Store/PayPalReturn.cshtml` exists and is accessible

### Authorize.Net: Payment declined
**Cause**: Invalid card or declined by bank  
**Fix**: Check card details, use test card numbers in sandbox

## Next Steps

### For Developers:
1. ? Configure Program.cs with HttpClient and payment providers
2. ? Add credentials to appsettings.json
3. ? Create PayPalReturn page for PayPal integration
4. ? Update checkout logic to branch by provider type
5. ? Test both Authorize.Net and PayPal flows

### For Administrators:
1. Get API credentials from payment providers
2. Set `PaymentProvider` configuration key in database
3. Test transactions in sandbox/test mode
4. Monitor logs for payment processing
5. Switch to production credentials when ready

### For Operations:
1. Set up monitoring for payment failures
2. Configure alerts for capture failures (PayPal)
3. Implement background job to clean up abandoned orders
4. Monitor return URL accessibility
5. Review logs regularly for errors

## Extensibility

To add support for a new payment provider:

1. Create new provider class implementing `IPaymentProvider`
2. Register HttpClient for the provider
3. Add configuration in Program.cs `AddPaymentProviders`
4. Add credentials to appsettings.json
5. If redirect-based: Create redirect service and return page
6. Update checkout logic if needed
7. Set `PaymentProvider` configuration key to new provider name

Example: Adding Stripe
```csharp
builder.Services.AddHttpClient<StripeProvider>();

builder.Services.AddPaymentProviders(b => {
    b.ConfigureProvider("Stripe", config => {
        config.ApiKey = configuration["PaymentProviders:Stripe:ApiKey"];
        config.ApiSecret = configuration["PaymentProviders:Stripe:ApiSecret"];
        config.IsTestMode = configuration.GetValue<bool>("PaymentProviders:Stripe:IsTestMode");
    });
});
```

## Documentation

Complete documentation available in:
- **README.md**: Full API documentation
- **INTEGRATION_GUIDE.md**: Step-by-step integration
- **PAYPAL_INTEGRATION.md**: PayPal-specific guide with redirect flow
- **QUICK_REFERENCE.md**: Quick reference and cheat sheet
- **PaymentProviderConfigurationSteps.md**: Administrator setup guide
- **BEST_PRACTICES.md**: Security and implementation best practices

## Support and Troubleshooting

**Provider Not Configured**:
- Ensure `PaymentProvider` configuration key exists in database
- Verify value is set to a valid provider name
- Check application logs

**Payment Processing Fails**:
- Review error code and message from provider
- Check API credentials in appsettings.json
- Verify test vs. production environment setting
- For PayPal: Check return page is accessible

**Configuration Not Taking Effect**:
- Application may need to restart to load new configuration
- Check Configuration admin panel to verify value saved
- Review application logs for configuration loading

---

**Status**: ? Implementation Complete and Tested  
**Build**: ? Successful  
**Model**: ? Single Administrator-Configured Provider with Dual Integration Patterns  
**Providers**: ? Authorize.Net (Direct) + PayPal (REST API v2 with Redirect)  
**Ready for**: Production Deployment
