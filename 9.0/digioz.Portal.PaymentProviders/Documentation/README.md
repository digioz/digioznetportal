# digioz.Portal.PaymentProviders

A reusable, extensible payment provider library for .NET 9.0 that supports multiple payment gateways using abstraction and dependency injection.

## Features

- **Abstraction-Based Architecture**: Built on interfaces and abstract classes for maximum extensibility
- **Built-in Providers**: Authorize.net and PayPal implementations included
- **Dependency Injection Ready**: Seamless integration with Microsoft.Extensions.DependencyInjection
- **Factory Pattern**: Easy provider selection and instantiation
- **Async/Await Support**: All operations are fully asynchronous
- **Comprehensive Models**: Request/Response models with validation
- **Error Handling**: Detailed error information and response codes

## Supported Payment Providers

### Authorize.net
- Transaction type: AUTH_CAPTURE (charge and hold)
- Refunds: Full and partial refunds supported
- API: AIM (Advanced Integration Method)

### PayPal
- Transaction type: Sale (charge and settle)
- Refunds: Full and partial refunds supported
- API: NVP (Name-Value Pair) Direct Payment

## Installation

### Quick Start

Add the library to your project and configure it in your service startup:

```csharp
services.AddPaymentProviders(builder =>
{
    builder.ConfigureProvider("AuthorizeNet", config =>
    {
        config.ApiKey = "YOUR_AUTHORIZE_NET_LOGIN";
        config.ApiSecret = "YOUR_AUTHORIZE_NET_TRANSACTION_KEY";
        config.IsTestMode = true;
    });
    
    builder.ConfigureProvider("PayPal", config =>
    {
        config.ApiKey = "YOUR_PAYPAL_API_USERNAME";
        config.ApiSecret = "YOUR_PAYPAL_API_PASSWORD";
        config.MerchantId = "YOUR_PAYPAL_API_SIGNATURE";
        config.IsTestMode = true;
    });
});
```

## Usage

### Using the Factory

```csharp
public class CheckoutService
{
    private readonly IPaymentProviderFactory _factory;
    
    public CheckoutService(IPaymentProviderFactory factory)
    {
        _factory = factory;
    }
    
    public async Task<PaymentResponse> ProcessPaymentAsync(
        string providerName, 
        CheckOutViewModel model)
    {
        var provider = _factory.CreateProvider(providerName);
        
        var request = new PaymentRequest
        {
            TransactionId = Guid.NewGuid().ToString(),
            Amount = model.Total * 100, // In cents
            CurrencyCode = "USD",
            CardNumber = model.CCNumber,
            ExpirationMonth = model.CCExpMonth,
            ExpirationYear = model.CCExpYear,
            CardCode = model.CCCardCode,
            CardholderName = $"{model.FirstName} {model.LastName}",
            Description = "Store Purchase",
            CustomerEmail = model.Email,
            CustomerPhone = model.Phone,
            BillingAddress = model.BillingAddress,
            BillingCity = model.BillingCity,
            BillingState = model.BillingState,
            BillingZip = model.BillingZip,
            BillingCountry = model.BillingCountry,
            ShippingAddress = model.ShippingAddress,
            ShippingCity = model.ShippingCity,
            ShippingState = model.ShippingState,
            ShippingZip = model.ShippingZip,
            ShippingCountry = model.ShippingCountry,
            InvoiceNumber = model.InvoiceNumber
        };
        
        return await provider.ProcessPaymentAsync(request);
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
    // Log and notify user
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
        public override string Name => "Stripe";
        
        public override bool ValidateConfiguration()
        {
            return !string.IsNullOrWhiteSpace(Config?.ApiKey) &&
                   !string.IsNullOrWhiteSpace(Config?.ApiSecret);
        }
        
        public override async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            // Implementation here
            throw new NotImplementedException();
        }
        
        public override async Task<PaymentResponse> RefundAsync(string transactionId, decimal? amount = null)
        {
            // Implementation here
            throw new NotImplementedException();
        }
    }
}
```

2. **Register in DI Container**:

```csharp
services.AddPaymentProviders(builder =>
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
| ApiKey | string | Primary API credential (login/username) |
| ApiSecret | string | Secondary API credential (password/key) |
| MerchantId | string | Merchant identifier (PayPal signature) |
| IsTestMode | bool | Use sandbox/test environment |
| Options | Dictionary | Additional provider-specific settings |

### PaymentRequest Properties

All properties on `PaymentRequest` are documented with XML comments. Key properties:

- `TransactionId`: Unique identifier for the request
- `Amount`: Transaction amount (in smallest currency unit)
- `CurrencyCode`: ISO 4217 currency code (default: "USD")
- `CardNumber`, `ExpirationMonth`, `ExpirationYear`, `CardCode`: Card details
- `CardholderName`: Name on card
- `BillingAddress*`: Billing address fields
- `ShippingAddress*`: Shipping address fields
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

### Design Patterns Used

- **Strategy Pattern**: Interchangeable payment provider implementations
- **Factory Pattern**: Creating provider instances
- **Dependency Injection**: Decoupling and testability
- **Template Method**: Base class defines structure, subclasses implement specifics

## Error Handling

All async methods return a `PaymentResponse` which includes error information. Check the `IsApproved` property first:

```csharp
var response = await provider.ProcessPaymentAsync(request);

if (!response.IsApproved)
{
    Console.WriteLine($"Error Code: {response.ErrorCode}");
    Console.WriteLine($"Error Message: {response.ErrorMessage}");
}
```

## Security Considerations

- Never log full card numbers or CVV codes
- Always use HTTPS for payment processing
- Store credentials in secure configuration (secrets manager, Key Vault)
- Validate input before processing
- Implement PCI DSS compliance measures
- Consider tokenization for recurring payments

## Testing

### Unit Testing Example

```csharp
[TestClass]
public class PaymentProviderTests
{
    [TestMethod]
    public async Task ProcessPayment_WithValidRequest_ReturnsApprovedResponse()
    {
        // Arrange
        var config = new PaymentProviderConfig
        {
            ApiKey = "TEST_API_KEY",
            ApiSecret = "TEST_API_SECRET",
            IsTestMode = true
        };
        
        var provider = new AuthorizeNetProvider();
        provider.Initialize(config);
        
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

## Future Enhancements

- Square payment provider
- 2Checkout provider
- Cryptocurrency providers
- Webhooks for async notifications
- Tokenization/Vault support
- 3D Secure/EMV verification
- Subscription/recurring billing
- Mobile wallet support (Apple Pay, Google Pay)

## License

As part of the digioz Portal project.

## Support

For issues, questions, or contributions, please contact the development team or submit issues through the project repository.
