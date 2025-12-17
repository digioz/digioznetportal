# Integration Guide for digioz.Portal.PaymentProviders

This guide explains how to integrate the `digioz.Portal.PaymentProviders` library into the main digioz Portal web application.

## Step 1: Update Project Reference

Add a reference to the payment providers library in `digioz.Portal.Web.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\digioz.Portal.PaymentProviders\digioz.Portal.PaymentProviders.csproj" />
</ItemGroup>
```

## Step 2: Configure in Program.cs

In your `Program.cs` (or `Startup.cs` for older .NET versions), add the payment providers configuration:

```csharp
using digioz.Portal.PaymentProviders.Examples;
using digioz.Portal.PaymentProviders.DependencyInjection;

var builder = WebApplicationBuilder.CreateBuilder(args);

// ... existing service registrations ...

// Add payment providers with configuration from appsettings.json
builder.Services.ConfigurePaymentProviders(builder.Configuration);

// Or use environment variables:
// builder.Services.ConfigurePaymentProvidersFromEnvironment();

var app = builder.Build();

// ... rest of your application setup ...
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
      "ApiKey": "YOUR_PAYPAL_API_USERNAME",
      "ApiSecret": "YOUR_PAYPAL_API_PASSWORD",
      "MerchantId": "YOUR_PAYPAL_API_SIGNATURE",
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
      "ApiKey": "YOUR_PRODUCTION_USERNAME",
      "ApiSecret": "YOUR_PRODUCTION_PASSWORD",
      "MerchantId": "YOUR_PRODUCTION_SIGNATURE",
      "IsTestMode": false
    }
  }
}
```

## Step 4: Create Payment Processing Service

Create a new service in your application to handle payment processing. Here's an example:

```csharp
using digioz.Portal.PaymentProviders.Abstractions;
using digioz.Portal.PaymentProviders.Models;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Services
{
    public interface IOrderPaymentService
    {
        Task<PaymentResponse> ProcessOrderPaymentAsync(Order order, CheckOutViewModel checkout);
        Task<PaymentResponse> RefundOrderPaymentAsync(Order order, decimal? refundAmount = null);
    }

    public class OrderPaymentService : IOrderPaymentService
    {
        private readonly IPaymentProviderFactory _paymentFactory;
        private readonly ILogger<OrderPaymentService> _logger;

        public OrderPaymentService(
            IPaymentProviderFactory paymentFactory,
            ILogger<OrderPaymentService> logger)
        {
            _paymentFactory = paymentFactory;
            _logger = logger;
        }

        public async Task<PaymentResponse> ProcessOrderPaymentAsync(
            Order order,
            CheckOutViewModel checkout)
        {
            try
            {
                var provider = _paymentFactory.CreateProvider(checkout.PaymentGateway);

                var request = new PaymentRequest
                {
                    TransactionId = order.Id,
                    Amount = (long)(order.Total * 100), // Convert to cents
                    CurrencyCode = "USD",
                    CardNumber = checkout.CCNumber,
                    ExpirationMonth = checkout.CCExpMonth,
                    ExpirationYear = checkout.CCExpYear,
                    CardCode = checkout.CCCardCode,
                    CardholderName = $"{checkout.FirstName} {checkout.LastName}",
                    CustomerEmail = checkout.Email,
                    CustomerPhone = checkout.Phone,
                    BillingAddress = checkout.BillingAddress,
                    BillingCity = checkout.BillingCity,
                    BillingState = checkout.BillingState,
                    BillingZip = checkout.BillingZip,
                    BillingCountry = checkout.BillingCountry,
                    ShippingAddress = checkout.ShippingAddress,
                    ShippingCity = checkout.ShippingCity,
                    ShippingState = checkout.ShippingState,
                    ShippingZip = checkout.ShippingZip,
                    ShippingCountry = checkout.ShippingCountry,
                    InvoiceNumber = order.InvoiceNumber,
                    Description = "Portal Store Purchase"
                };

                var response = await provider.ProcessPaymentAsync(request);

                _logger.LogInformation(
                    "Payment processed for order {OrderId} via {Provider}: Approved={IsApproved}",
                    order.Id,
                    checkout.PaymentGateway,
                    response.IsApproved);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for order {OrderId}", order.Id);
                throw;
            }
        }

        public async Task<PaymentResponse> RefundOrderPaymentAsync(
            Order order,
            decimal? refundAmount = null)
        {
            try
            {
                var paymentGateway = DeterminePaymentGateway(order); // Implement based on your logic
                var provider = _paymentFactory.CreateProvider(paymentGateway);

                var amount = refundAmount.HasValue ? (long)(refundAmount.Value * 100) : (long?)null;
                var response = await provider.RefundAsync(order.TrxId, (decimal?)amount);

                _logger.LogInformation(
                    "Refund processed for order {OrderId}: Approved={IsApproved}",
                    order.Id,
                    response.IsApproved);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding order {OrderId}", order.Id);
                throw;
            }
        }

        private string DeterminePaymentGateway(Order order)
        {
            // Implement logic to determine which gateway was used for this order
            // Could be stored in the order or retrieved from configuration
            return "AuthorizeNet"; // Default
        }
    }
}
```

Register the service in Program.cs:

```csharp
builder.Services.AddScoped<IOrderPaymentService, OrderPaymentService>();
```

## Step 5: Update Checkout Page Handler

Update your checkout Razor Page to use the new payment service:

```csharp
using digioz.Portal.Web.Services;
using digioz.Portal.Bo.ViewModels;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly IOrderPaymentService _paymentService;
        private readonly IOrderService _orderService; // Your existing service
        private readonly ILogger<CheckoutModel> _logger;

        [BindProperty]
        public CheckOutViewModel CheckOut { get; set; }

        public CheckoutModel(
            IOrderPaymentService paymentService,
            IOrderService orderService,
            ILogger<CheckoutModel> logger)
        {
            _paymentService = paymentService;
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Create order
                var order = new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = User.FindFirst("sub")?.Value,
                    OrderDate = DateTime.Now,
                    FirstName = CheckOut.FirstName,
                    LastName = CheckOut.LastName,
                    Email = CheckOut.Email,
                    Phone = CheckOut.Phone,
                    ShippingAddress = CheckOut.ShippingAddress,
                    ShippingCity = CheckOut.ShippingCity,
                    ShippingState = CheckOut.ShippingState,
                    ShippingZip = CheckOut.ShippingZip,
                    ShippingCountry = CheckOut.ShippingCountry,
                    BillingAddress = CheckOut.BillingAddress,
                    BillingCity = CheckOut.BillingCity,
                    BillingState = CheckOut.BillingState,
                    BillingZip = CheckOut.BillingZip,
                    BillingCountry = CheckOut.BillingCountry,
                    Total = 100.00m, // Calculate from cart
                    Ccnumber = CheckOut.CCNumber,
                    Ccexp = $"{CheckOut.CCExpMonth}/{CheckOut.CCExpYear}",
                    CccardCode = CheckOut.CCCardCode,
                    TrxDescription = "Store Purchase"
                };

                // Process payment
                var paymentResponse = await _paymentService.ProcessOrderPaymentAsync(order, CheckOut);

                if (paymentResponse.IsApproved)
                {
                    // Update order with transaction details
                    order.TrxApproved = true;
                    order.TrxAuthorizationCode = paymentResponse.AuthorizationCode;
                    order.TrxId = paymentResponse.TransactionId;
                    order.TrxMessage = paymentResponse.Message;
                    order.TrxResponseCode = paymentResponse.ResponseCode;

                    // Save order
                    await _orderService.SaveOrderAsync(order);

                    // Clear cart, send confirmation email, etc.
                    // ...

                    return RedirectToPage("OrderConfirmation", new { orderId = order.Id });
                }
                else
                {
                    ModelState.AddModelError("", $"Payment declined: {paymentResponse.ErrorMessage}");
                    _logger.LogWarning("Payment declined for order: {ErrorCode} - {ErrorMessage}",
                        paymentResponse.ErrorCode,
                        paymentResponse.ErrorMessage);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred processing your payment. Please try again.");
                _logger.LogError(ex, "Error processing checkout");
                return Page();
            }
        }
    }
}
```

## Step 6: Update CheckOutViewModel (Optional)

Add a property for payment gateway selection if not already present:

```csharp
[Required]
[StringLength(40)]
[DisplayName("Payment Gateway")]
public string PaymentGateway { get; set; } = "AuthorizeNet";
```

Add to your checkout view:

```html
<div class="form-group">
    <label asp-for="PaymentGateway"></label>
    <select asp-for="PaymentGateway" class="form-control">
        <option value="AuthorizeNet">Authorize.net</option>
        <option value="PayPal">PayPal</option>
    </select>
</div>
```

## Step 7: Error Handling

Implement proper error handling and logging throughout your payment processing:

```csharp
try
{
    var response = await paymentService.ProcessOrderPaymentAsync(order, checkout);
    
    if (!response.IsApproved)
    {
        // Handle declined payment
        await LogPaymentFailure(order.Id, response);
        TempData["Error"] = "Payment was declined. Please check your card details and try again.";
    }
}
catch (ArgumentException ex)
{
    // Handle invalid configuration or invalid request
    await LogPaymentError(order.Id, ex);
    TempData["Error"] = "Payment gateway is not properly configured.";
}
catch (Exception ex)
{
    // Handle unexpected errors
    await LogPaymentError(order.Id, ex);
    TempData["Error"] = "An unexpected error occurred. Please try again later.";
}
```

## Step 8: Security Considerations

1. **Never store credit card data directly in the database** - Use tokenization instead
2. **Use HTTPS** - All payment communication should be encrypted
3. **Store credentials securely** - Use Azure Key Vault, AWS Secrets Manager, or similar
4. **Implement PCI DSS compliance** - Validate and maintain compliance
5. **Log sensitive data carefully** - Never log full card numbers or CVV codes
6. **Implement rate limiting** - Prevent brute force attacks on payment endpoints

## Testing

For development and testing:

1. Use test/sandbox credentials in `appsettings.Development.json`
2. Use the Mock provider for unit tests
3. Test with provider-specific test card numbers:
   - **Authorize.net**: 4111111111111111 (approved), 4222222222222220 (declined)
   - **PayPal**: See their sandbox testing documentation

Example unit test:

```csharp
[TestClass]
public class OrderPaymentServiceTests
{
    [TestMethod]
    public async Task ProcessOrderPayment_WithValidPayment_ReturnsApproved()
    {
        // Arrange
        var mockFactory = new Mock<IPaymentProviderFactory>();
        var mockProvider = new Mock<IPaymentProvider>();
        var mockLogger = new Mock<ILogger<OrderPaymentService>>();

        mockFactory.Setup(f => f.CreateProvider("AuthorizeNet"))
            .Returns(mockProvider.Object);

        mockProvider.Setup(p => p.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(new PaymentResponse { IsApproved = true });

        var service = new OrderPaymentService(mockFactory.Object, mockLogger.Object);

        // Act & Assert
        // Your test code here
    }
}
```

## Migration from Old Payment System

If you have an existing payment processing implementation:

1. Keep both implementations running in parallel
2. Add feature flags to enable the new payment providers gradually
3. Log both old and new results for comparison
4. Once verified, remove the old implementation

```csharp
if (featureFlags.UseNewPaymentProviders)
{
    var response = await _paymentService.ProcessOrderPaymentAsync(order, checkout);
}
else
{
    var response = await _legacyPaymentService.ProcessOrderPaymentAsync(order, checkout);
}
```

## Support and Troubleshooting

### Provider Not Found
- Ensure the provider name matches the registration (case-insensitive)
- Check that configuration is properly loaded
- Verify the provider is registered in the DI container

### Configuration Issues
- Verify credentials in appsettings.json
- Check that `IsTestMode` matches your account type
- Use environment-specific configuration files

### Payment Failures
- Check the `ErrorCode` and `ErrorMessage` in the response
- Review provider-specific error codes in their documentation
- Log all transaction details for debugging

## Additional Resources

- [Authorize.net Documentation](https://developer.authorize.net/)
- [PayPal Documentation](https://developer.paypal.com/)
- [Payment PCI DSS Compliance](https://www.pcisecuritystandards.org/)

