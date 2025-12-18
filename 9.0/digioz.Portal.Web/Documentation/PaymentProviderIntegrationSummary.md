# Payment Provider Integration Summary

## Overview
The digioz Portal web application now integrates the `digioz.Portal.PaymentProviders` library to use a **single, administrator-configured payment provider** for all transactions. The administrator chooses which payment provider (Authorize.net or PayPal) to use, and users do not select the payment method during checkout.

## Key Points

? **Administrator Control**: The administrator determines which payment provider is used for all transactions  
? **Single Provider**: Only one payment provider can be active at a time  
? **Centralized Configuration**: Payment provider is set via application configuration (Config database table)  
? **No User Selection**: Users do not see payment provider options during checkout  
? **Automatic Processing**: Payment is processed using the configured provider transparently

## How Payment Provider Selection Works

### Decision Flow

```
User proceeds to checkout
         ?
Application retrieves payment provider from config
         ?
Validates provider is registered
         ?
Factory creates provider instance
         ?
Selected provider processes payment
         ?
Transaction details saved to Order
```

## Changes Made

### 1. **CheckOutViewModel** (digioz.Portal.Bo/ViewModels/CheckOutViewModel.cs)
**Removed**: `PaymentGateway` property (no longer user-selected)

### 2. **Checkout Page UI** (digioz.Portal.Web/Pages/Store/Checkout.cshtml)
**Removed**: Payment gateway dropdown selector from checkout form

### 3. **Checkout Page Code-Behind** (digioz.Portal.Web/Pages/Store/Checkout.cshtml.cs)

**Removed**:
- `[BindProperty] public string PaymentGateway` - No longer needed
- Validation for user payment provider selection

**Added**:
- `GetConfiguredPaymentProvider()` method - Retrieves configured provider from database
- Automatic payment provider selection based on admin configuration

## Configuration

### Setting the Payment Provider (Admin Task)

The administrator sets the payment provider by adding/updating a configuration entry:

**Configuration Key**: `PaymentProvider`  
**Configuration Value**: `AuthorizeNet` or `PayPal`

**Database Entry**:
```sql
INSERT INTO Config (Id, ConfigKey, ConfigValue, [Description])
VALUES (
    NEWID(),
    'PaymentProvider',
    'AuthorizeNet',  -- or 'PayPal'
    'The payment provider used for all transactions'
)
```

**Via Admin Interface** (if available):
- Navigate to Admin ? Configuration
- Add/Edit key: `PaymentProvider`
- Set value: `AuthorizeNet` or `PayPal`
- Save

### Changing the Payment Provider

To switch payment providers (e.g., from Authorize.net to PayPal):
1. Go to Admin Configuration panel
2. Find the `PaymentProvider` configuration key
3. Change the value to the new provider name
4. Save changes

The change takes effect immediately for all new transactions.

## Implementation Details

### GetConfiguredPaymentProvider() Method

```csharp
/// <summary>
/// Gets the configured payment provider name from application settings.
/// The administrator determines which single payment provider is used for all transactions.
/// </summary>
/// <returns>The configured payment provider name (e.g., "AuthorizeNet", "PayPal"), or null if not configured</returns>
private string GetConfiguredPaymentProvider() {
    // Check for payment provider configuration in app settings
    var config = _configService.GetByKey("PaymentProvider");
    if (config != null && !string.IsNullOrEmpty(config.ConfigValue)) {
        return config.ConfigValue.Trim();
    }

    // Log if no configuration found
    _logger.LogWarning("PaymentProvider configuration key not found in settings");
    return null;
}
```

### ProcessPaymentAsync() Flow

1. **Retrieves** configured provider from settings
2. **Validates** provider is registered in DI container
3. **Creates** provider instance using factory
4. **Builds** payment request with transaction details
5. **Processes** payment asynchronously
6. **Returns** response with transaction details

## Error Handling

| Scenario | Behavior |
|----------|----------|
| Provider not configured | Error: "Payment provider is not configured" |
| Provider not registered | Error: "Payment provider is not configured" |
| Card processing fails | Error from payment provider (e.g., declined) |
| Network/System error | Error: "An error occurred processing your payment" |

## Benefits

? **Simplified User Experience**: Users see no payment method selection  
? **Centralized Control**: Admin controls payment processing without code changes  
? **Easier Maintenance**: Single provider means one integration to maintain  
? **Flexibility**: Administrator can switch providers via configuration  
? **Security**: No need for client-side provider selection logic

## Testing

### Prerequisites
- Payment provider configured in Config table
- API credentials configured in Program.cs
- Test card numbers from the configured provider

### Test Flow
1. Add product to cart
2. Proceed to checkout
3. Fill in customer information and card details
4. Submit order
5. Verify payment is processed by configured provider
6. Check transaction details saved to Order

## Security Considerations

? **Configuration Storage**: Provider name stored in secure configuration  
? **Logging**: Payment attempts logged with provider name  
? **Credentials**: API credentials stored in Program.cs configuration (not in UI)  
? **Audit Trail**: All transactions logged with provider details

## Future Enhancements

- Multi-provider failover (if provider is unavailable, use backup)
- Provider-specific settings per provider type
- Transaction history filtered by provider
- Provider analytics and reporting
