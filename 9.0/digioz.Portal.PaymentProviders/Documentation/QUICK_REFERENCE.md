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
      "ApiKey": "YOUR_USERNAME",
      "ApiSecret": "YOUR_PASSWORD",
      "MerchantId": "YOUR_SIGNATURE",
      "IsTestMode": true
    }
  }
}
```

### 2. Update Program.cs
```csharp
using digioz.Portal.PaymentProviders.Examples;

builder.Services.ConfigurePaymentProviders(builder.Configuration);
```

### 3. Use in Your Service
```csharp
public class CheckoutService
{
    private readonly IPaymentProviderFactory _factory;
    
    public async Task<PaymentResponse> ProcessPaymentAsync(
        string providerName, 
        PaymentRequest request)
    {
        var provider = _factory.CreateProvider(providerName);
        return await provider.ProcessPaymentAsync(request);
    }
}
```

## Common Operations

### Process a Payment
```csharp
var provider = _factory.CreateProvider("AuthorizeNet");

var request = new PaymentRequest
{
    TransactionId = Guid.NewGuid().ToString(),
    Amount = 10000, // In cents
    CardNumber = "4111111111111111",
    ExpirationMonth = "12",
    ExpirationYear = "2025",
    CardCode = "123",
    CardholderName = "John Doe",
    // ... other fields
};

var response = await provider.ProcessPaymentAsync(request);

if (response.IsApproved)
{
    // Success - save transaction details
    order.TrxId = response.TransactionId;
    order.TrxApproved = true;
}
else
{
    // Failed - show error to user
    errorMessage = response.ErrorMessage;
}
```

### Refund a Payment
```csharp
var provider = _factory.CreateProvider("AuthorizeNet");

// Full refund
var refund = await provider.RefundAsync("TRANSACTION_ID");

// Partial refund
var partialRefund = await provider.RefundAsync("TRANSACTION_ID", 50.00m);

if (refund.IsApproved)
{
    // Refund processed
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

## Configuration Options

### PaymentProviderConfig
```csharp
var config = new PaymentProviderConfig
{
    ApiKey = "...",           // Login/Username
    ApiSecret = "...",        // Password/Key
    MerchantId = "...",       // Merchant ID (PayPal)
    IsTestMode = true,        // Use sandbox
    Options = new Dictionary<string, string>
    {
        { "timeout", "30000" },
        { "retryCount", "3" }
    }
};
```

### Register Custom Provider
```csharp
services.AddPaymentProviders(builder =>
{
    builder.AddProvider<StripeProvider>("Stripe")
           .ConfigureProvider("Stripe", config =>
           {
               config.ApiKey = "sk_test_...";
               config.ApiSecret = "pk_test_...";
           });
});
```

## PaymentRequest Properties Cheat Sheet

| Property | Type | Example |
|----------|------|---------|
| TransactionId | string | "ORDER-12345" |
| Amount | decimal | 10000 (cents) |
| CurrencyCode | string | "USD" |
| CardNumber | string | "4111111111111111" |
| ExpirationMonth | string | "12" |
| ExpirationYear | string | "2025" |
| CardCode | string | "123" |
| CardholderName | string | "John Doe" |
| Description | string | "Store Purchase" |
| CustomerEmail | string | "john@example.com" |
| CustomerPhone | string | "+1234567890" |
| BillingAddress | string | "123 Main St" |
| BillingCity | string | "New York" |
| BillingState | string | "NY" |
| BillingZip | string | "10001" |
| BillingCountry | string | "US" |
| ShippingAddress | string | "456 Elm St" |
| ShippingCity | string | "Los Angeles" |
| ShippingState | string | "CA" |
| ShippingZip | string | "90001" |
| ShippingCountry | string | "US" |
| InvoiceNumber | string | "INV-001" |
| CustomFields | Dictionary | Additional data |

## PaymentResponse Properties Cheat Sheet

| Property | Type | Meaning |
|----------|------|---------|
| IsApproved | bool | Transaction succeeded |
| AuthorizationCode | string | Auth code from provider |
| TransactionId | string | Provider's transaction ID |
| Message | string | Provider response message |
| ResponseCode | string | Code like "1" (approved) |
| Amount | decimal | Amount charged |
| ErrorMessage | string | Error description |
| ErrorCode | string | Error code from provider |
| RawResponse | Dictionary | Full provider response |
| CustomData | Dictionary | Provider-specific data |

## Test Card Numbers

### Authorize.net
- **Approved**: 4111111111111111
- **Declined**: 4222222222222220

### PayPal Sandbox
- Check PayPal sandbox documentation for test credentials

### Mock Provider
- **Approved**: 4111111111111111
- **Declined**: 4000000000000002

## Error Handling Pattern

```csharp
try
{
    var response = await provider.ProcessPaymentAsync(request);
    
    if (!response.IsApproved)
    {
        // Handle declined payment
        logger.LogWarning("Payment declined: {Code}", response.ErrorCode);
        return new { success = false, message = response.ErrorMessage };
    }
    
    // Process successful payment
    return new { success = true, transactionId = response.TransactionId };
}
catch (ArgumentException ex)
{
    // Invalid request
    logger.LogError(ex, "Invalid payment request");
    return new { success = false, message = "Invalid payment information" };
}
catch (Exception ex)
{
    // Unexpected error
    logger.LogError(ex, "Payment processing error");
    return new { success = false, message = "Please try again later" };
}
```

## DI Container Registration

```csharp
// Default (Authorize.net + PayPal)
services.AddPaymentProviders();

// Custom configuration
services.AddPaymentProviders(builder =>
{
    builder.ConfigureProvider("AuthorizeNet", config =>
    {
        config.ApiKey = GetFromSecrets("AuthorizeNet:Key");
        config.ApiSecret = GetFromSecrets("AuthorizeNet:Secret");
        config.IsTestMode = !IsProd;
    });
});

// Manual provider
services.AddPaymentProvider<AuthorizeNetProvider>(
    "AuthorizeNet", 
    new PaymentProviderConfig
    {
        ApiKey = "...",
        ApiSecret = "..."
    });
```

## Environment Variables

```bash
PAYMENT_AUTHORIZENET_APIKEY=your_key
PAYMENT_AUTHORIZENET_APISECRET=your_secret
PAYMENT_AUTHORIZENET_TESTMODE=true

PAYMENT_PAYPAL_APIKEY=your_username
PAYMENT_PAYPAL_APISECRET=your_password
PAYMENT_PAYPAL_MERCHANTID=your_signature
PAYMENT_PAYPAL_TESTMODE=true
```

## Logging & Monitoring

```csharp
// Log payment attempt
logger.LogInformation("Processing payment: {Provider}, Amount: {Amount}",
    providerName, request.Amount);

// Log response
logger.LogInformation("Payment result: Approved={IsApproved}, TxId={TransactionId}",
    response.IsApproved, response.TransactionId);

// Log error
logger.LogError("Payment failed: {ErrorCode} - {ErrorMessage}",
    response.ErrorCode, response.ErrorMessage);

// Monitor metrics
metrics.RecordPaymentAttempt(providerName, response.IsApproved);
```

## Validation Checklist Before Production

- [ ] API credentials configured correctly
- [ ] IsTestMode = false for production
- [ ] HTTPS enabled
- [ ] Error handling implemented
- [ ] Logging configured (no card data)
- [ ] Payment response saved to database
- [ ] Refund process tested
- [ ] Provider test cards work
- [ ] Monitoring/alerts setup
- [ ] Documentation reviewed

## Troubleshooting

### Provider not found
```
ArgumentException: Payment provider 'ProviderName' is not registered.
```
**Fix**: Check provider name matches registration (case-insensitive) and configuration is loaded.

### Payment fails with invalid credentials
```
Response: IsApproved = false, ErrorCode = "INVALID_CREDENTIALS"
```
**Fix**: Verify ApiKey, ApiSecret, and IsTestMode match your provider account type.

### Configuration is null
```
NullReferenceException: Object reference not set to an instance of an object.
```
**Fix**: Ensure appsettings.json has PaymentProviders section and ConfigurePaymentProviders is called.

### Timeout errors
```
OperationCanceledException: The operation was canceled.
```
**Fix**: Check network connectivity, increase timeout in configuration, or retry with exponential backoff.

## Documentation Files

- **README.md**: Full API documentation and features
- **INTEGRATION_GUIDE.md**: Detailed integration steps
- **BEST_PRACTICES.md**: Security and implementation guidelines
- **FILE_MANIFEST.md**: Complete file listing
- **IMPLEMENTATION_SUMMARY.md**: Project overview

## Provider Documentation Links

- [Authorize.net Developer Docs](https://developer.authorize.net/)
- [PayPal Developer Docs](https://developer.paypal.com/)

## Key Classes

```csharp
// Main interfaces
IPaymentProvider          // Implement to add providers
IPaymentProviderFactory   // Get provider instances

// Data models
PaymentRequest           // Input to ProcessPaymentAsync
PaymentResponse          // Output from ProcessPaymentAsync
PaymentProviderConfig    // Configuration for providers

// Built-in providers
AuthorizeNetProvider     // Authorize.net implementation
PayPalProvider           // PayPal implementation
MockPaymentProvider      // Testing provider

// DI helpers
ServiceCollectionExtensions  // Register providers
PaymentProviderBuilder       // Configure providers
```

## Next Steps

1. **Integration**: Follow INTEGRATION_GUIDE.md
2. **Configuration**: Set up credentials in appsettings.json
3. **Testing**: Use mock provider or test cards
4. **Monitoring**: Implement logging and metrics
5. **Production**: Verify all security requirements

---

**Need Help?**
- Check README.md for detailed documentation
- Review INTEGRATION_GUIDE.md for step-by-step instructions
- Consult BEST_PRACTICES.md for security guidelines
- Look at Examples/ folder for usage samples
