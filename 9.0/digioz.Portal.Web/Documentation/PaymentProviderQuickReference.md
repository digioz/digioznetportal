# Quick Reference: Payment Provider Configuration (Administrator Model)

## What Changed

The payment provider selection model has been changed from **user selection during checkout** to **administrator configuration**.

### Before
- Users selected payment provider (Authorize.net or PayPal) during checkout
- Checkout form had payment provider dropdown
- CheckOutViewModel had PaymentGateway property

### After ?
- Administrator selects payment provider via configuration
- Users do not see provider options during checkout
- Payment processing is transparent to users
- Only one provider is active at a time

## Code Changes

### 1. CheckOutViewModel (Bo Project)
```csharp
// REMOVED: No longer user-selected
// public string PaymentGateway { get; set; } = "AuthorizeNet";
```

### 2. Checkout Page (Razor)
```html
<!-- REMOVED: Payment method dropdown no longer in UI -->
```

### 3. Checkout Code-Behind
```csharp
// REMOVED: User payment gateway binding
// [BindProperty]
// public string PaymentGateway { get; set; } = "AuthorizeNet";

// NEW: Retrieve from configuration
private string GetConfiguredPaymentProvider() {
    var config = _configService.GetByKey("PaymentProvider");
    if (config != null && !string.IsNullOrEmpty(config.ConfigValue)) {
        return config.ConfigValue.Trim();
    }
    return null;
}
```

## Configuration (3 Steps)

### Step 1: Update Program.cs (Developer)
```csharp
builder.Services.AddPaymentProviders(builder =>
{
    builder.ConfigureProvider("AuthorizeNet", config =>
    {
        config.ApiKey = builder.Configuration["PaymentProviders:AuthorizeNet:ApiKey"];
        config.ApiSecret = builder.Configuration["PaymentProviders:AuthorizeNet:ApiSecret"];
        config.IsTestMode = !builder.Environment.IsProduction();
    });
    
    builder.ConfigureProvider("PayPal", config =>
    {
        config.ApiKey = builder.Configuration["PaymentProviders:PayPal:ApiKey"];
        config.ApiSecret = builder.Configuration["PaymentProviders:PayPal:ApiSecret"];
        config.MerchantId = builder.Configuration["PaymentProviders:PayPal:MerchantId"];
        config.IsTestMode = !builder.Environment.IsProduction();
    });
});
```

### Step 2: Configure appsettings.json (Developer)
```json
{
  "PaymentProviders": {
    "AuthorizeNet": {
      "ApiKey": "YOUR_LOGIN_ID",
      "ApiSecret": "YOUR_TRANSACTION_KEY"
    },
    "PayPal": {
      "ApiKey": "YOUR_API_USERNAME",
      "ApiSecret": "YOUR_API_PASSWORD",
      "MerchantId": "YOUR_API_SIGNATURE"
    }
  }
}
```

### Step 3: Set Active Provider (Administrator)

**Option A: Via Admin Panel**
1. Log in as Administrator
2. Go to Admin ? Configuration (or Settings)
3. Create/Edit configuration key: `PaymentProvider`
4. Set value to: `AuthorizeNet` or `PayPal`
5. Save

**Option B: Via SQL**
```sql
-- Insert new configuration
INSERT INTO Config (Id, ConfigKey, ConfigValue, [Description])
VALUES (NEWID(), 'PaymentProvider', 'AuthorizeNet', 'Payment provider for all transactions')

-- Or update existing
UPDATE Config SET ConfigValue = 'PayPal' WHERE ConfigKey = 'PaymentProvider'
```

## How It Works

### User Workflow
```
1. User adds products to cart
   ?
2. User goes to checkout
   ?
3. User fills billing/shipping info
   ?
4. User enters card details
   ?
5. User clicks "Place Order and Pay"
   ?
6. System retrieves configured provider (e.g., "AuthorizeNet")
   ?
7. System creates payment request
   ?
8. AuthorizeNet processes payment
   ?
9. If approved: Order saved, redirect to confirmation
   If declined: Show error, user can retry
```

### Admin Workflow
```
1. Admin opens Admin Panel
   ?
2. Navigate to Configuration
   ?
3. Find/Create "PaymentProvider" key
   ?
4. Set value to desired provider
   ?
5. Save
   ?
6. All new transactions use this provider
```

## Changing Providers

To switch from Authorize.net to PayPal:

**Admin Panel**:
1. Edit the `PaymentProvider` configuration key
2. Change value from `AuthorizeNet` to `PayPal`
3. Save
4. Done! All new transactions now use PayPal

**SQL**:
```sql
UPDATE Config SET ConfigValue = 'PayPal' WHERE ConfigKey = 'PaymentProvider'
```

## Key Classes

### IPaymentProviderFactory
- **Purpose**: Create payment provider instances
- **Location**: `digioz.Portal.PaymentProviders.Abstractions`
- **Methods**:
  - `CreateProvider(name)` - Creates provider by name
  - `IsProviderAvailable(name)` - Checks if provider registered

### GetConfiguredPaymentProvider()
- **Purpose**: Retrieve configured provider from database
- **Returns**: Provider name ("AuthorizeNet", "PayPal", etc.) or null
- **Source**: Config table, key="PaymentProvider"

### PaymentRequest
- **Purpose**: Contains payment details
- **Key Properties**:
  - Card: CardNumber, ExpirationMonth, ExpirationYear, CardCode
  - Customer: CardholderName, Email, Phone
  - Addresses: BillingAddress, ShippingAddress, etc.

### PaymentResponse
- **Purpose**: Contains payment result
- **Key Properties**:
  - `IsApproved` - bool
  - `TransactionId` - string
  - `AuthorizationCode` - string
  - `ErrorMessage` - string (if failed)

## Testing

### Test Card Numbers

**Authorize.net Sandbox**:
- Approved: 4111111111111111
- Declined: 4222222222222220
- Expiration: Any future date
- CVV: Any 3 digits

**PayPal Sandbox**:
- See PayPal sandbox documentation

### Basic Test
1. Configure provider in database (set PaymentProvider = "AuthorizeNet")
2. Add product to cart
3. Go to checkout
4. Enter test card and details
5. Submit order
6. Verify payment processed successfully

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Payment provider is not configured" | Add PaymentProvider key to Config table |
| Payment fails | Check API credentials in appsettings.json |
| Wrong provider used | Verify PaymentProvider config value is correct |
| Configuration not taking effect | Restart application to reload config |

## Benefits

? **Simplified Checkout**: Users don't select provider  
? **Admin Control**: One configuration controls all payments  
? **Easy Switching**: Change providers via admin panel  
? **Consistent Experience**: Same provider for all users  
? **Maintainable**: Single integration to manage  

## Files Modified
? `digioz.Portal.Bo/ViewModels/CheckOutViewModel.cs`  
? `digioz.Portal.Web/Pages/Store/Checkout.cshtml`  
? `digioz.Portal.Web/Pages/Store/Checkout.cshtml.cs`  

## Configuration Status
? Code changes complete  
? Ready for Program.cs setup (developer)  
? Ready for appsettings.json setup (developer)  
? Ready for PaymentProvider configuration (administrator)  

---

For detailed documentation, see:
- `IMPLEMENTATION_COMPLETE_SUMMARY.md` - Complete technical details
- `PAYMENT_PROVIDER_INTEGRATION_SUMMARY.md` - Architecture and design
- `PAYMENT_PROVIDER_CONFIGURATION_STEPS.md` - Step-by-step setup guide
