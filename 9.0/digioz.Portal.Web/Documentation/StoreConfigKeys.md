# Store System Configuration Keys

The following configuration keys need to be added to the `Config` table to support the e-commerce store system. These can be added via the Admin panel or via SQL.

## Payment Configuration Keys

### 1. PaymentProviderType
- **ConfigKey:** `PaymentProviderType`
- **ConfigValue:** One of: `Stripe`, `PayPal`, `Square`
- **Description:** Determines which payment provider will be used for processing transactions
- **Example SQL:**
  ```sql
  INSERT INTO Config (Id, ConfigKey, ConfigValue)
  VALUES (NEWID(), 'PaymentProviderType', 'Stripe')
  ```

### 2. Stripe Configuration Keys
Required if using Stripe as payment provider:

#### Stripe API Key (Secret)
- **ConfigKey:** `StripeSecretKey`
- **ConfigValue:** Your Stripe secret API key
- **Example SQL:**
  ```sql
  INSERT INTO Config (Id, ConfigKey, ConfigValue)
  VALUES (NEWID(), 'StripeSecretKey', 'sk_test_...')
  ```

#### Stripe Publishable Key
- **ConfigKey:** `StripePublishableKey`
- **ConfigValue:** Your Stripe publishable API key
- **Example SQL:**
  ```sql
  INSERT INTO Config (Id, ConfigKey, ConfigValue)
  VALUES (NEWID(), 'StripePublishableKey', 'pk_test_...')
  ```

### 3. PayPal Configuration Keys
Required if using PayPal as payment provider:

#### PayPal Client ID
- **ConfigKey:** `PayPalClientId`
- **ConfigValue:** Your PayPal Client ID
- **Example SQL:**
  ```sql
  INSERT INTO Config (Id, ConfigKey, ConfigValue)
  VALUES (NEWID(), 'PayPalClientId', 'AbX...')
  ```

#### PayPal Secret
- **ConfigKey:** `PayPalSecret`
- **ConfigValue:** Your PayPal Secret Key
- **Example SQL:**
  ```sql
  INSERT INTO Config (Id, ConfigKey, ConfigValue)
  VALUES (NEWID(), 'PayPalSecret', 'EK1...')
  ```

### 4. Square Configuration Keys
Required if using Square as payment provider:

#### Square Access Token
- **ConfigKey:** `SquareAccessToken`
- **ConfigValue:** Your Square Access Token
- **Example SQL:**
  ```sql
  INSERT INTO Config (Id, ConfigKey, ConfigValue)
  VALUES (NEWID(), 'SquareAccessToken', 'sq0atp_...')
  ```

#### Square Location ID
- **ConfigKey:** `SquareLocationId`
- **ConfigValue:** Your Square Location ID
- **Example SQL:**
  ```sql
  INSERT INTO Config (Id, ConfigKey, ConfigValue)
  VALUES (NEWID(), 'SquareLocationId', 'L...')
  ```

## Store Configuration Keys

### Order Email From Address
- **ConfigKey:** `OrderEmailFromAddress`
- **ConfigValue:** Email address to send order confirmations from
- **Example SQL:**
  ```sql
  INSERT INTO Config (Id, ConfigKey, ConfigValue)
  VALUES (NEWID(), 'OrderEmailFromAddress', 'orders@yourdomain.com')
  ```

### Store Name
- **ConfigKey:** `StoreName`
- **ConfigValue:** Display name of your store
- **Example SQL:**
  ```sql
  INSERT INTO Config (Id, ConfigKey, ConfigValue)
  VALUES (NEWID(), 'StoreName', 'My Online Store')
  ```

### Store Currency
- **ConfigKey:** `StoreCurrency`
- **ConfigValue:** ISO 4217 currency code (e.g., USD, EUR, GBP)
- **Example SQL:**
  ```sql
  INSERT INTO Config (Id, ConfigKey, ConfigValue)
  VALUES (NEWID(), 'StoreCurrency', 'USD')
  ```

## Implementation Notes

1. **Payment Processing:**
   - The `Checkout.cshtml.cs` page includes placeholder methods for each payment provider:
     - `ProcessStripePaymentAsync()`
     - `ProcessPayPalPaymentAsync()`
     - `ProcessSquarePaymentAsync()`
   - These methods should be implemented based on your chosen payment provider's API documentation

2. **Configuration Retrieval:**
   - Configuration keys are retrieved using: `_configService.GetByKey("KeyName")`
   - Always validate that configuration keys exist before using them
   - Store sensitive keys (API keys, secrets) securely, preferably using Azure Key Vault or similar service in production

3. **Testing:**
   - Use test/sandbox credentials when developing and testing
   - Switch to production credentials only after thorough testing

## Security Recommendations

1. **Never store API keys in source code** - Use environment variables or secure configuration management
2. **Use HTTPS** for all payment transactions
3. **Implement PCI-DSS compliance** when handling payment data
4. **Regular security audits** of payment processing code
5. **Log payment activities** for audit trails (without storing sensitive card data)
6. **Keep payment provider libraries up to date** with security patches
