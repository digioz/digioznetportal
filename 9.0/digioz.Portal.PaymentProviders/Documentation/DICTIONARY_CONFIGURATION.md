# Dictionary Configuration Guide

## Overview

In addition to the existing `appsettings.json` and environment variable configuration methods, the Payment Providers library now supports configuration via dictionaries. This provides maximum flexibility for dynamic configuration scenarios.

## Configuration Methods Comparison

| Method | Source | Best For | Use Case |
|--------|--------|----------|----------|
| **appsettings.json** | File | Standard ASP.NET Core apps | Configuration files, deployment-specific settings |
| **Environment Variables** | OS Environment | Containerized apps, CI/CD | Docker, Kubernetes, cloud deployments |
| **Dictionary** | Programmatic | Dynamic configuration | Runtime settings, feature flags, testing |

## Dictionary Configuration Format

### Basic Format

Dictionary keys are provider names, and values are comma-separated key=value pairs:

```csharp
var configs = new Dictionary<string, string>
{
    { "AuthorizeNet", "ApiKey=LOGIN_ID,ApiSecret=TRANSACTION_KEY,IsTestMode=true" },
    { "PayPal", "ApiKey=USERNAME,ApiSecret=PASSWORD,MerchantId=SIGNATURE,IsTestMode=false" }
};

services.AddPaymentProvidersFromDictionary(configs);
```

### Supported Configuration Keys

| Key | Type | Required | Description |
|-----|------|----------|-------------|
| `ApiKey` | string | Yes (most providers) | API key or username for authentication |
| `ApiSecret` | string | Yes | API secret, password, or transaction key |
| `MerchantId` | string | No | Merchant ID (PayPal specific) |
| `IsTestMode` | bool | No | Use sandbox/test environment (default: true) |
| Custom Keys | string | No | Any custom settings stored in Options dictionary |

## Usage Examples

### Example 1: Basic Dictionary Configuration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

var paymentConfigs = new Dictionary<string, string>
{
    { "AuthorizeNet", "ApiKey=2425He3P46,ApiSecret=32r9Q2fKj8,IsTestMode=true" },
    { "PayPal", "ApiKey=seller@example.com,ApiSecret=mypassword123,MerchantId=ABC123XYZ,IsTestMode=true" }
};

builder.Services.AddPaymentProvidersFromDictionary(paymentConfigs);

var app = builder.Build();
```

### Example 2: Dynamic Configuration from Database

```csharp
// Load settings from database at runtime
public class PaymentConfigurationService
{
    public void ConfigurePaymentProviders(IServiceCollection services)
    {
        var configs = new Dictionary<string, string>();
        
        // Load from database
        foreach (var provider in _database.PaymentProviders)
        {
            var settings = string.Join(",", provider.Settings.Select(s => $"{s.Key}={s.Value}"));
            configs[provider.Name] = settings;
        }
        
        services.AddPaymentProvidersFromDictionary(configs);
    }
}
```

### Example 3: Configuration from AppSettings but Modifiable

```csharp
// Program.cs
var configuration = builder.Configuration;
var paymentSection = configuration.GetSection("PaymentProviders");

var configs = new Dictionary<string, string>
{
    { 
        "AuthorizeNet", 
        $"ApiKey={paymentSection["AuthorizeNet:ApiKey"]}," +
        $"ApiSecret={paymentSection["AuthorizeNet:ApiSecret"]}," +
        $"IsTestMode={paymentSection["AuthorizeNet:IsTestMode"]}" 
    },
    { 
        "PayPal", 
        $"ApiKey={paymentSection["PayPal:ApiKey"]}," +
        $"ApiSecret={paymentSection["PayPal:ApiSecret"]}," +
        $"MerchantId={paymentSection["PayPal:MerchantId"]}," +
        $"IsTestMode={paymentSection["PayPal:IsTestMode"]}" 
    }
};

services.AddPaymentProvidersFromDictionary(configs);
```

### Example 4: Using PaymentProviderStartup Helper

```csharp
// Program.cs
var paymentConfigs = new Dictionary<string, string>
{
    { "AuthorizeNet", "ApiKey=test_key,ApiSecret=test_secret,IsTestMode=true" },
    { "PayPal", "ApiKey=test_user,ApiSecret=test_pwd,MerchantId=test_merchant,IsTestMode=true" }
};

builder.Services.ConfigurePaymentProvidersFromDictionary(paymentConfigs);
```

### Example 5: Conditional Configuration Based on Environment

```csharp
// Program.cs
var environment = builder.Environment.EnvironmentName;

var configs = new Dictionary<string, string>();

if (environment == "Production")
{
    configs["AuthorizeNet"] = "ApiKey=prod_key,ApiSecret=prod_secret,IsTestMode=false";
    configs["PayPal"] = "ApiKey=prod_user,ApiSecret=prod_pwd,MerchantId=prod_merchant,IsTestMode=false";
}
else
{
    configs["AuthorizeNet"] = "ApiKey=test_key,ApiSecret=test_secret,IsTestMode=true";
    configs["PayPal"] = "ApiKey=test_user,ApiSecret=test_pwd,MerchantId=test_merchant,IsTestMode=true";
}

services.AddPaymentProvidersFromDictionary(configs);
```

## Advanced Configuration

### Custom Settings Storage

Unknown configuration keys are automatically stored in the provider's `Options` dictionary for custom use:

```csharp
var configs = new Dictionary<string, string>
{
    { "CustomProvider", "ApiKey=key,ApiSecret=secret,CustomParam1=value1,CustomParam2=value2" }
};

services.AddPaymentProvidersFromDictionary(configs);

// In your provider, access via:
var customValue = Config.Options["CustomParam1"]; // "value1"
```

### Case-Insensitive Key Matching

Configuration keys are case-insensitive:

```csharp
// All of these are equivalent:
"ApiKey=value"
"apikey=value"
"APIKEY=value"
"ApiKey=value"
```

### Whitespace Handling

Leading and trailing whitespace is automatically trimmed:

```csharp
// All of these are equivalent:
"ApiKey=value1,ApiSecret=value2"
"ApiKey = value1 , ApiSecret = value2"
"  ApiKey  =  value1  ,  ApiSecret  =  value2  "
```

## Error Handling

The dictionary configuration includes validation:

```csharp
try
{
    // Null dictionary
    services.AddPaymentProvidersFromDictionary(null);
    // Throws: ArgumentNullException
}
catch (ArgumentNullException) { }

try
{
    // Empty provider name
    var configs = new Dictionary<string, string> { { "", "ApiKey=value" } };
    // Throws: ArgumentException
}
catch (ArgumentException) { }

try
{
    // Empty configuration string
    var configs = new Dictionary<string, string> { { "AuthorizeNet", "" } };
    // Throws: ArgumentException
}
catch (ArgumentException) { }
```

## Combining Configuration Methods

You can combine multiple configuration methods in the same application:

```csharp
// Start with appsettings.json
services.ConfigurePaymentProviders(configuration);

// Override or add to with dictionary at runtime
var runtimeConfigs = new Dictionary<string, string>
{
    { "AuthorizeNet", "ApiKey=override_key,ApiSecret=override_secret,IsTestMode=true" }
};

// Note: This would add providers again, so typically use one method
// Alternatively, use the builder approach:
services.AddPaymentProviders(builder =>
{
    builder.ConfigureProvider("AuthorizeNet", config =>
    {
        config.ApiKey = "dynamic_key";
        config.ApiSecret = "dynamic_secret";
    });
});
```

## Razor Pages Integration Example

For a Razor Pages application, you might configure payment providers in `Program.cs`:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages
builder.Services.AddRazorPages();

// Configure payment providers from dictionary
var paymentConfigs = GetPaymentConfigurationsFromSource();
builder.Services.AddPaymentProvidersFromDictionary(paymentConfigs);

// Add payment processing service
builder.Services.AddScoped<PaymentProcessingService>();

var app = builder.Build();
app.MapRazorPages();
app.Run();

private static Dictionary<string, string> GetPaymentConfigurationsFromSource()
{
    return new Dictionary<string, string>
    {
        { "AuthorizeNet", "ApiKey=...,ApiSecret=...,IsTestMode=true" },
        { "PayPal", "ApiKey=...,ApiSecret=...,MerchantId=...,IsTestMode=true" }
    };
}
```

Then use in your Razor Page:

```csharp
// Pages/Checkout.cshtml.cs
public class CheckoutModel : PageModel
{
    private readonly PaymentProcessingService _paymentService;

    public CheckoutModel(PaymentProcessingService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var response = await _paymentService.ProcessPaymentAsync(
            "AuthorizeNet",
            amount: 99.99m,
            cardNumber: Model.CardNumber,
            // ... other parameters
        );
        
        if (response.IsApproved)
        {
            // Success
        }
        else
        {
            ModelState.AddModelError("", response.ErrorMessage);
        }

        return Page();
    }
}
```

## Performance Considerations

- **Dictionary parsing is one-time**: Configuration is parsed during application startup, not on each request
- **Minimal overhead**: The string parsing and key matching is negligible
- **Lazy loading**: Providers are only instantiated when first requested via `CreateProvider()`

## Migration Guide

### From appsettings.json to Dictionary

**Before:**
```json
{
  "PaymentProviders": {
    "AuthorizeNet": {
      "ApiKey": "key123",
      "ApiSecret": "secret456",
      "IsTestMode": true
    }
  }
}
```

```csharp
services.ConfigurePaymentProviders(configuration);
```

**After:**
```csharp
var configs = new Dictionary<string, string>
{
    { "AuthorizeNet", "ApiKey=key123,ApiSecret=secret456,IsTestMode=true" }
};

services.AddPaymentProvidersFromDictionary(configs);
```

### From Environment Variables to Dictionary

**Before:**
```bash
PAYMENT_AUTHORIZENET_APIKEY=key123
PAYMENT_AUTHORIZENET_APISECRET=secret456
PAYMENT_AUTHORIZENET_TESTMODE=true
```

```csharp
services.ConfigurePaymentProvidersFromEnvironment();
```

**After:**
```csharp
var configs = new Dictionary<string, string>
{
    { 
        "AuthorizeNet", 
        $"ApiKey={Environment.GetEnvironmentVariable("PAYMENT_AUTHORIZENET_APIKEY")}," +
        $"ApiSecret={Environment.GetEnvironmentVariable("PAYMENT_AUTHORIZENET_APISECRET")}," +
        $"IsTestMode={Environment.GetEnvironmentVariable("PAYMENT_AUTHORIZENET_TESTMODE")}"
    }
};

services.AddPaymentProvidersFromDictionary(configs);
```

## Troubleshooting

### Configuration Not Applied

**Problem**: Settings don't seem to be applied
**Solution**: Ensure the dictionary keys match provider names exactly:
```csharp
// Must match registered provider name
var configs = new Dictionary<string, string>
{
    { "AuthorizeNet", "ApiKey=value,..." }  // ? Correct
    { "authorizenet", "ApiKey=value,..." }  // ? May not match
};
```

### Missing Configuration Error

**Problem**: "Payment provider 'X' is not properly configured"
**Solution**: Verify all required fields are present:
```csharp
// Make sure ApiKey and ApiSecret are provided
{ "AuthorizeNet", "ApiKey=value,ApiSecret=value,IsTestMode=true" }  // ?
{ "AuthorizeNet", "ApiKey=value,IsTestMode=true" }  // ? Missing ApiSecret
```

### Boolean Parsing Failed

**Problem**: IsTestMode is not being set correctly
**Solution**: Use valid boolean values:
```csharp
"IsTestMode=true"   // ?
"IsTestMode=false"  // ?
"IsTestMode=1"      // ? Not parsed
"IsTestMode=yes"    // ? Not parsed
```

## See Also

- [QUICK_REFERENCE.md](QUICK_REFERENCE.md) - 5-minute quick start
- [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md) - Detailed integration instructions
- [README.md](README.md) - Complete API documentation
