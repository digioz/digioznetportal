# digioz.Portal.PaymentProviders

A reusable, extensible payment provider library for .NET 9.0 that supports multiple payment gateways using abstraction and dependency injection.

## Features

- **Abstraction-Based Architecture**: Built on interfaces and abstract classes for maximum extensibility
- **Built-in Providers**: 
  - **Authorize.net**: Direct card processing (AIM API)
  - **PayPal**: REST API with redirect-based approval flow
- **Dependency Injection Ready**: Seamless integration with Microsoft.Extensions.DependencyInjection
- **Factory Pattern**: Easy provider selection and instantiation
- **Async/Await Support**: All operations are fully asynchronous
- **Comprehensive Models**: Request/Response models with validation
- **Error Handling**: Detailed error information and response codes
- **Dual Integration Patterns**: Supports both direct card processing and redirect-based approval flows

## Supported Payment Providers

### Authorize.net
- **Transaction type**: AUTH_CAPTURE (charge and hold)
- **Integration**: Direct server-side card processing
- **API**: AIM (Advanced Integration Method)
- **Refunds**: Full and partial refunds supported
- **User Experience**: Single-page checkout

### PayPal (REST API)
- **Transaction type**: Order + Capture
- **Integration**: Redirect-based approval flow
- **API**: REST API v2 (Orders API)
- **Refunds**: Full and partial refunds supported
- **User Experience**: Redirect to PayPal for approval, then return
- **Configuration**: ClientId and ClientSecret (OAuth 2.0)
- **Return URLs**: Dynamically generated based on request host

## Installation

### Quick Start

Add the library to your project and configure it in your service startup:

```csharp
// Configure HttpClient for providers
builder.Services.AddHttpClient<AuthorizeNetProvider>();
builder.Services.AddHttpClient<PayPalProvider>();

// Add payment providers
builder.Services.AddPaymentProviders(builder =>
{
    builder.ConfigureProvider("AuthorizeNet", config =>
    {
        config.ApiKey = "YOUR_AUTHORIZE_NET_LOGIN";
        config.ApiSecret = "YOUR_AUTHORIZE_NET_TRANSACTION_KEY";
        config.IsTestMode = true;
    });
    
    builder.ConfigureProvider("PayPal", config =>
    {
        config.ApiKey = "YOUR_PAYPAL_CLIENT_ID";       // ClientId
        config.ApiSecret = "YOUR_PAYPAL_CLIENT_SECRET"; // ClientSecret
        config.IsTestMode = true;
    });
});

// Register PayPal redirect service
builder.Services.AddScoped<IPayPalRedirectService, PayPalRedirectService>();
```

## Usage

### Direct Card Processing (Authorize.net)

```csharp
public class CheckoutService
{
    private readonly IPaymentProviderFactory _factory;
    
    public CheckoutService(IPaymentProviderFactory factory)
    {
        _factory = factory;
    }
    
    public async Task<PaymentResponse> ProcessPaymentAsync(CheckOutViewModel model)
    {
        var provider = _factory.CreateProvider("AuthorizeNet");
        
        var request = new PaymentRequest
        {
            TransactionId = order.Id,
            Amount = order.Total,
            CurrencyCode = "USD",
            CardNumber = model.CCNumber,
            ExpirationMonth = model.CCExpMonth,
            ExpirationYear = model.CCExpYear,
            CardCode = model.CCCardCode,
            CardholderName = $"{model.FirstName} {model.LastName}",
            CustomerEmail = model.Email,
            // ... other fields
        };
        
        return await provider.ProcessPaymentAsync(request);
    }
}
```

### Redirect-Based Processing (PayPal)

```csharp
public class CheckoutModel : PageModel
{
    private readonly IPayPalRedirectService _payPalRedirectService;
    private readonly IOrderService _orderService;

    public async Task<IActionResult> OnPostAsync()
    {
        // Save pending order to database
        Order.TrxApproved = false;
        Order.TrxResponseCode = "PENDING";
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

        // Get dynamic return URL
        var returnBaseUrl = $"{Request.Scheme}://{Request.Host}";

        // Create PayPal order
        var (paypalOrderId, approveUrl) = await _payPalRedirectService
            .CreateOrderAsync(request, returnBaseUrl);

        // Store PayPal order ID
        Order.TrxId = paypalOrderId;
        _orderService.Update(Order);

        // Redirect user to PayPal
        return Redirect(approveUrl);
    }
}

// PayPal return handler
public class PayPalReturnModel : PageModel
{
    private readonly IPayPalRedirectService _payPalRedirectService;
    private readonly IOrderService _orderService;

    public async Task<IActionResult> OnGetAsync([FromQuery] string token)
    {
        // Find pending order by PayPal token
        var order = _orderService.GetAll()
            .Where(o => o.TrxId == token && o.TrxResponseCode == "PENDING")
            .FirstOrDefault();

        // Capture the approved payment
        var response = await _payPalRedirectService.CaptureOrderAsync(token, request);

        if (response.IsApproved)
        {
            order.TrxApproved = true;
            order.TrxId = response.TransactionId;
            _orderService.Update(order);
            return RedirectToPage("OrderConfirmation");
        }

        return RedirectToPage("Checkout");
    }
}
```

### Handling Responses

```csharp
var response = await provider.ProcessPaymentAsync(request);

if (response.IsApproved)
{
    // Save transaction details
    order.TrxApproved = true;
    order.TrxAuthorizationCode = response.AuthorizationCode;
    order.TrxId = response.TransactionId;
    order.TrxMessage = response.Message;
}
else
{
    // Handle error
    var errorMessage = response.ErrorMessage ?? response.Message;
    _logger.LogWarning("Payment declined: {ErrorCode} - {ErrorMessage}", 
        response.ErrorCode, errorMessage);
}
```

### Issuing Refunds

```csharp
var refundResponse = await provider.RefundAsync(
    transactionId: "ORIGINAL_TRANSACTION_ID",
    amount: 50.00m // Optional: partial refund amount
);

if (refundResponse.IsApproved)
{
    // Refund processed successfully
}
```

## Adding Custom Payment Providers

To add support for a new payment provider:

1. **Create a Provider Class**:

```csharp
namespace digioz.Portal.PaymentProviders.Providers
{
    public class StripeProvider : BasePaymentProvider
    {
        private readonly HttpClient _httpClient;

        public override string Name => "Stripe";
        
        public StripeProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override bool ValidateConfiguration()
        {
            return !string.IsNullOrWhiteSpace(Config?.ApiKey) &&
                   !string.IsNullOrWhiteSpace(Config?.ApiSecret);
        }
        
        public override async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            // Implementation here
        }
        
        public override async Task<PaymentResponse> RefundAsync(string transactionId, decimal? amount = null)
        {
            // Implementation here
        }
    }
}
```

2. **Register in DI Container**:

```csharp
builder.Services.AddHttpClient<StripeProvider>();

builder.Services.AddPaymentProviders(builder =>
{
    builder.AddProvider<StripeProvider>("Stripe")
           .ConfigureProvider("Stripe", config =>
           {
               config.ApiKey = "sk_test_...";
               config.ApiSecret = "pk_test_...";
               config.IsTestMode = true;
           });
});
```

## Configuration

### PaymentProviderConfig Properties

| Property | Type | Description |
|----------|------|-------------|
| ApiKey | string | Primary API credential (login/username/client_id) |
| ApiSecret | string | Secondary API credential (password/key/client_secret) |
| MerchantId | string | Merchant identifier (optional, provider-specific) |
| IsTestMode | bool | Use sandbox/test environment |
| Options | Dictionary | Additional provider-specific settings |

### PaymentRequest Properties

- `TransactionId`: Unique identifier for the request
- `Amount`: Transaction amount (decimal dollars for PayPal, cents for some providers)
- `CurrencyCode`: ISO 4217 currency code (default: "USD")
- `CardNumber`, `ExpirationMonth`, `ExpirationYear`, `CardCode`: Card details (for direct processing)
- `CardholderName`: Name on card
- `BillingAddress*`: Billing address fields
- `ShippingAddress*`: Shipping address fields
- `InvoiceNumber`: Invoice/order reference
- `Description`: Transaction description
- `CustomFields`: Additional data for specific providers

### PaymentResponse Properties

| Property | Type | Description |
|----------|------|-------------|
| IsApproved | bool | Transaction approval status |
| AuthorizationCode | string | Auth code from provider |
| TransactionId | string | Provider's transaction ID |
| Message | string | Provider response message |
| ResponseCode | string | Provider response code |
| Amount | decimal | Charged amount |
| ErrorMessage | string | Error details if failed |
| ErrorCode | string | Provider error code |
| RawResponse | Dictionary | Complete provider response |
| CustomData | Dictionary | Provider-specific additional data |

## Architecture

### Key Components

- **IPaymentProvider**: Interface that all providers must implement
- **BasePaymentProvider**: Abstract base class with common functionality
- **IPaymentProviderFactory**: Factory interface for creating providers
- **PaymentProviderFactory**: Factory implementation supporting DI
- **Models**: `PaymentRequest`, `PaymentResponse`, `PaymentProviderConfig`
- **IPayPalRedirectService**: Service for PayPal redirect flow

### Design Patterns Used

- **Strategy Pattern**: Interchangeable payment provider implementations
- **Factory Pattern**: Creating provider instances
- **Dependency Injection**: Decoupling and testability
- **Template Method**: Base class defines structure, subclasses implement specifics
- **Service Pattern**: PayPal redirect service encapsulates multi-step flow

## Error Handling

All async methods return a `PaymentResponse` which includes error information. Check the `IsApproved` property first:

```csharp
var response = await provider.ProcessPaymentAsync(request);

if (!response.IsApproved)
{
    _logger.LogWarning("Payment declined: {ErrorCode} - {ErrorMessage}",
        response.ErrorCode, response.ErrorMessage);
    // Display error to user
}
```

## Security Considerations

- **Never log full card numbers or CVV codes**
- **Always use HTTPS** for payment processing
- **Store credentials securely** (Azure Key Vault, AWS Secrets Manager)
- **Validate input** before processing
- **Implement PCI DSS compliance** for card processing
- **Validate PayPal return URLs** to prevent spoofing
- **Clean up pending orders** to prevent database bloat
- **Consider tokenization** for recurring payments

## Testing

### Unit Testing Example

```csharp
[TestClass]
public class PaymentProviderTests
{
    [TestMethod]
    public async Task AuthorizeNet_ProcessPayment_WithValidRequest_ReturnsApproved()
    {
        // Arrange
        var mockHttpClient = new HttpClient(new MockHttpMessageHandler());
        var provider = new AuthorizeNetProvider(mockHttpClient);
        
        provider.Initialize(new PaymentProviderConfig
        {
            ApiKey = "TEST_API_KEY",
            ApiSecret = "TEST_API_SECRET",
            IsTestMode = true
        });
        
        var request = new PaymentRequest
        {
            Amount = 100.00m,
            CardNumber = "4111111111111111",
            ExpirationMonth = "12",
            ExpirationYear = "2025",
            CardCode = "123"
        };
        
        // Act
        var response = await provider.ProcessPaymentAsync(request);
        
        // Assert
        Assert.IsTrue(response.IsApproved);
    }
}
```

### Integration Testing

1. Use sandbox/test credentials
2. Test full redirect flow for PayPal
3. Verify return URL handling
4. Test error scenarios
5. Validate refund functionality

## Future Enhancements

- Stripe payment provider
- Square provider
- 2Checkout provider
- Cryptocurrency providers
- Webhooks for async notifications
- Tokenization/Vault support
- 3D Secure/EMV verification
- Subscription/recurring billing
- Mobile wallet support (Apple Pay, Google Pay)

## Support

For issues, questions, or contributions, please contact the development team or submit issues through the project repository.

## License

Part of the digioz Portal project.
