# Payment Provider Configuration - Administrator Guide

## Overview

The digioz Portal uses a single payment provider configured by the administrator. Users do not select a payment method during checkout; the system uses the provider configured by the admin.

## Configuration Steps

### Step 1: Add Project Reference (Developer Task)

If not already done, add the PaymentProviders reference to `digioz.Portal.Web.csproj`:

```xml
<ProjectReference Include="..\digioz.Portal.PaymentProviders\digioz.Portal.PaymentProviders.csproj" />
```

### Step 2: Register Payment Providers in Program.cs (Developer Task)

Add to `Program.cs` (around the other service configuration):

```csharp
using digioz.Portal.PaymentProviders.DependencyInjection;

// In the service configuration section:
builder.Services.AddPaymentProviders(builder =>
{
    // Configure AuthorizeNet
    var authorizeNetConfig = builder.Configuration.GetSection("PaymentProviders:AuthorizeNet");
    if (authorizeNetConfig.Exists())
    {
        builder.ConfigureProvider("AuthorizeNet", config =>
        {
            config.ApiKey = authorizeNetConfig["ApiKey"];
            config.ApiSecret = authorizeNetConfig["ApiSecret"];
            config.IsTestMode = !builder.Environment.IsProduction();
        });
    }

    // Configure PayPal (REST-style naming: ClientId / ClientSecret)
    var paypalConfig = builder.Configuration.GetSection("PaymentProviders:PayPal");
    if (paypalConfig.Exists())
    {
        builder.ConfigureProvider("PayPal", config =>
        {
            // Map to generic fields; provider interprets these as needed
            config.ApiKey = paypalConfig["ClientId"];      // PayPal REST ClientId
            config.ApiSecret = paypalConfig["ClientSecret"]; // PayPal REST ClientSecret

            // Optional classic NVP signature can go into Options if still used
            var signature = paypalConfig["Signature"];
            if (!string.IsNullOrWhiteSpace(signature))
            {
                config.Options["Signature"] = signature;
            }

            config.IsTestMode = !builder.Environment.IsProduction();
        });
    }
});
```

### Step 3: Configure API Credentials in appsettings.json (Developer Task)

Add to `appsettings.json`:

```json
{
  "PaymentProviders": {
    "AuthorizeNet": {
      "ApiKey": "YOUR_AUTHORIZE_NET_LOGIN_ID",
      "ApiSecret": "YOUR_AUTHORIZE_NET_TRANSACTION_KEY"
    },
    "PayPal": {
      "ClientId": "YOUR_PAYPAL_REST_CLIENT_ID",
      "ClientSecret": "YOUR_PAYPAL_REST_CLIENT_SECRET"
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
      "ApiSecret": "YOUR_SANDBOX_TRANSACTION_KEY"
    },
    "PayPal": {
      "ClientId": "YOUR_SANDBOX_PAYPAL_CLIENT_ID",
      "ClientSecret": "YOUR_SANDBOX_PAYPAL_CLIENT_SECRET"
    }
  }
}
```

If you still need classic NVP/SOAP credentials (legacy), you can optionally add:

```json
"PayPal": {
  "ClientId": "...",
  "ClientSecret": "...",
  "Signature": "YOUR_PAYPAL_API_SIGNATURE"
}
```

### Step 4: Set Active Payment Provider (Administrator Task)

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

## Changing the Payment Provider

To switch from one payment provider to another:

1. **Using Admin Panel**: Edit the `PaymentProvider` configuration value
2. **Using SQL**: Update the Config table with the new provider name
3. **Effective Immediately**: The change takes effect for all new transactions

### Supported Provider Names
- `AuthorizeNet` - Authorize.net payment gateway
- `PayPal` - PayPal payment gateway

## Verification

To verify the payment provider is configured correctly:

1. Go to Admin ? Configuration
2. Look for the `PaymentProvider` key
3. Verify the value is set to your chosen provider
4. Check the application logs to confirm payment processing

## Test Card Numbers

### Authorize.net Sandbox
- **Approved**: 4111111111111111
- **Declined**: 4222222222222220
- **Expiration**: Any future date (MM/YY)
- **CVV**: Any 3 digits

### PayPal Sandbox
Refer to [PayPal Sandbox Documentation](https://developer.paypal.com/docs/classic/lifecycle/sb_test-accounts/)

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
- Check that API credentials are set in appsettings.json / environment
- Verify the provider is registered in Program.cs
- Check application logs for more details

### Payment Processing Fails
**Symptom**: Payments are declined or failing  
**Solution**:
- Verify API credentials in configuration are correct
- Check the environment (test vs. production) matches your credentials
- Verify IsTestMode setting in Program.cs matches your account type
- Check card details format and validity
- Review application logs for provider-specific error messages

## Configuration Checklist

### Developer Setup
- [ ] PaymentProviders project reference added to Web.csproj
- [ ] Program.cs updated with AddPaymentProviders registration
- [ ] API credentials added to configuration source (appsettings, env, Key Vault, etc.)
- [ ] API credentials added to `appsettings.Development.json` (for testing) or user secrets

### Administrator Setup
- [ ] Payment provider selected (Authorize.net or PayPal)
- [ ] PaymentProvider configuration key created with appropriate value
- [ ] Configuration verified in admin panel
- [ ] Test payment processed successfully

## Summary

The payment provider is configured centrally by the administrator. Users simply enter their card details during checkout, and the payment is processed by the configured provider automatically. To change providers, the administrator only needs to update one configuration value.
